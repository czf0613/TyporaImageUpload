//
//  UserRequests.swift
//  Liuxue
//
//  Created by 陈治帆 on 2022/10/6.
//

import Foundation

struct CreateUserRequest: Codable {
    let userTag: String
}

struct CreateUserResponse: Codable {
    let userTag: String
    let userId: Int
}
