//
//  main.swift
//  KCosUpload
//
//  Created by 陈治帆 on 2023/3/5.
//
import Foundation

@main
struct Cli {
    static func main() async {
        guard CommandLine.arguments.count > 1 else {
            print("Finished.")
            return
        }
        
        let appId = ""
        let appKey = ""
        let userTag = "macOS-command-line-tool"
        let sdkClient = KCosSDKClient(appId: appId, appKey: appKey)
        
        do {
            let _ = try await sdkClient.createOrFetchUser(userTag: userTag)
        } catch {
            print("获取用户Id失败，aborting...")
            return
        }
        
        let client = sdkClient.makeCosServiceClient()
        var ddl = Date()
        var readingFrom = 1
        
        if let maybeDDL = Int(CommandLine.arguments[1]) {
            ddl = ddl.addingTimeInterval(Double(maybeDDL) * 86400.0)
            readingFrom = 2
        } else {
            ddl = ddl.addingTimeInterval(5 * 365 * 86400.0)
        }
        
        let files = CommandLine.arguments.dropFirst(readingFrom)
        var results: [String] = []
        
        do {
            for i in files {
                let url = URL(fileURLWithPath: i)
                let (fileId, stream) = try await client.uploadFile(url, filePath: "/typora/macOS-tool/", fileNameWithExt: url.lastPathComponent, mimeType: Self.getMimeType(ext: url.lastPathComponent), deadLine: ddl)
                
                if stream != nil {
                    for await pack in stream! {
                        print("完成度：\(pack)%")
                    }
                }
                
                results.append("https://cos.kevinc.ltd/file/download?fileId=\(fileId)")
            }
        } catch {
            print("Failed")
            return
        }
        
        print("Upload Success:")
        for i in results {
            print(i)
        }
    }
    
    private static func getMimeType(ext: String) -> String {
        guard let lastDot = ext.lastIndex(of: ".") else {
            return "application/octet-stream"
        }
        
        let subString = ext[lastDot...].lowercased()
        
        switch subString {
        case ".jpeg":
            fallthrough
        case ".jpg":
            return "image/jpeg"
        case ".png":
            return "image/png"
        case ".webp":
            return "image/webp"
        case ".mp3":
            return "audio/mp3"
        case ".aac":
            return "audio/aac"
        case ".mp4":
            return "video/mp4"
        case ".mov":
            return "video/quicktime"
        case ".pdf":
            return "application/pdf"
        default:
            return "application/octet-stream"
        }
    }
}
