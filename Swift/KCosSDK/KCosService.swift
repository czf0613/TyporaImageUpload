//
//  KCosService.swift
//  Liuxue
//
//  Created by 陈治帆 on 2022/10/6.
//

import Foundation
import CryptoKit

struct KCosService {
    private let appId: String
    private let appKey: String
    private let userId: Int
    
    init(appId: String, appKey: String, userId: Int) {
        self.appId = appId
        self.appKey = appKey
        self.userId = userId
    }
    
    private func createFileEntry(filePath: String, fileNameWithExt: String, fileSize: Int64, sha256: String, mimeType: String = "application/octet-stream", deadLine: Date, protection: Int = 0, securityPayload: String = "") async throws -> (fileId: Int64, frames: Int, next: Int) {
        let requestBody = CreateFileEntryRequest(path: filePath, fileNameWithExt: fileNameWithExt, fileSize: fileSize, sha256: sha256, mimeType: mimeType, deadLine: deadLine, protection: protection, securityPayload: securityPayload)
        
        var request = URLRequest(url: URL(string: "\(KCosSDKClient.urlBase)/file/createFileEntry")!)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue(appId, forHTTPHeaderField: "X-AppId")
        request.setValue(appKey, forHTTPHeaderField: "X-AppKey")
        request.setValue("\(userId)", forHTTPHeaderField: "X-UserId")
        request.httpBody = try KCosSDKClient.jsonEncoder.encode(requestBody)
        
        let (data, _) = try await URLSession.shared.data(for: request)
        let responseBody = try KCosSDKClient.jsonDecoder.decode(CreateFileEntryResponse.self, from: data)
        
        return (responseBody.id, responseBody.frames, responseBody.nextRequestedFrame)
    }
    
    func uploadFile(_ file: URL, filePath: String, fileNameWithExt: String, mimeType: String = "application/octet-stream", deadLine: Date, protection: Int = 0, securityPayload: String = "") async throws -> (Int64, AsyncStream<Int>?) {
        let fileHandler = try FileHandle(forReadingFrom: file)
        let fileAttr = try FileManager.default.attributesOfItem(atPath: file.path)
        
        var hashDisgest = SHA256()
        var bytesRead = 0
        let fileSize = fileAttr[.size] as? Int ?? 0
        
        while bytesRead < fileSize {
            guard let data = try fileHandler.read(upToCount: 1048576) else {
                throw KCosError()
            }
            
            guard data.count > 0 else {
                continue
            }
            
            hashDisgest.update(data: data)
            bytesRead += data.count
        }
        
        let result = hashDisgest.finalize()
        let sha256 = result.map {
                String(format: "%02x", $0).lowercased()
            }
            .joined(separator: "")
        
        let (fileId, frames, nextFrame) = try await createFileEntry(filePath: filePath, fileNameWithExt: fileNameWithExt, fileSize: Int64(fileSize), sha256: sha256, mimeType: mimeType, deadLine: deadLine, protection: protection, securityPayload: securityPayload)
        
        guard nextFrame > 0 else {
            print("有重复文件存在，直接复用")
            return (fileId, nil)
        }
        
        try fileHandler.seek(toOffset: 0)
        
        return (fileId, AsyncStream { continuation in
            Task {
                var currentFrame = 0
                var bytes = 0
                do {
                    while bytes < fileSize {
                        guard let data = try fileHandler.read(upToCount: 1048576) else {
                            throw KCosError()
                        }
                        
                        guard data.count > 0 else {
                            continue
                        }
                        
                        if (data.count != 1048576) && (currentFrame != frames - 1) {
                            throw KCosError()
                        }
                        
                        var request = URLRequest(url: URL(string: "https://tcp-cos.kevinc.ltd:8080/file/upload?fileId=\(fileId)&seqNumber=\(currentFrame + 1)")!)
                        request.httpMethod = "PUT"
                        request.setValue("application/octet-stream", forHTTPHeaderField: "Content-Type")
                        request.setValue(appId, forHTTPHeaderField: "X-AppId")
                        request.setValue(appKey, forHTTPHeaderField: "X-AppKey")
                        request.setValue("\(userId)", forHTTPHeaderField: "X-UserId")
                        request.httpBody = data
                        
                        let (_, _) = try await URLSession.shared.data(for: request)
                        currentFrame += 1
                        bytes += data.count
                        print("KCos 已上传 \(currentFrame) / \(frames)")
                        
                        let percent = Double(currentFrame) / Double(frames) * 100.0
                        continuation.yield(Int(percent))
                    }
                    continuation.finish()
                } catch let err {
                    print("上传出错：\(err.localizedDescription)")
                    continuation.finish()
                }
            }
        })
    }
}
