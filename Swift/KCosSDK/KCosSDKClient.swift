//
//  KCosSDKClient.swift
//  Liuxue
//
//  Created by 陈治帆 on 2022/10/6.
//

import Foundation

final class KCosSDKClient {
    private let appId: String
    private let appKey: String
    private var userUid = 0
    static let urlBase = "https://tcp-cos.kevinc.ltd:8080"
    static let jsonDecoder = JSONDecoder()
    static let jsonEncoder = JSONEncoder()
    
    init(appId: String, appKey: String) {
        self.appId = appId
        self.appKey = appKey
        
        Self.jsonEncoder.dateEncodingStrategy = .iso8601
        Self.jsonDecoder.dateDecodingStrategy = .iso8601
    }
    
    func createOrFetchUser(userTag: String) async throws -> Int {
        let requestBody = CreateUserRequest(userTag: userTag)
        
        var request = URLRequest(url: URL(string: "\(Self.urlBase)/user/createAppUser")!)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue(appId, forHTTPHeaderField: "X-AppId")
        request.setValue(appKey, forHTTPHeaderField: "X-AppKey")
        
        do {
            request.httpBody = try Self.jsonEncoder.encode(requestBody)
            
            let (data, _) = try await URLSession.shared.data(for: request)
            let responseBody = try Self.jsonDecoder.decode(CreateUserResponse.self, from: data)
            self.userUid = responseBody.userId
            return self.userUid
        } catch let err {
            throw err
        }
    }
    
    func makeCosServiceClient() -> KCosService {
        return KCosService(appId: appId, appKey: appKey, userId: userUid)
    }
}
