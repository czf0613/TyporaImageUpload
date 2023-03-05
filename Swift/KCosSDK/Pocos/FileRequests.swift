//
//  FileRequests.swift
//  Liuxue
//
//  Created by 陈治帆 on 2022/10/6.
//

import Foundation

struct CreateFileEntryRequest: Codable {
    let path: String
    let fileNameWithExt: String
    let fileSize: Int64
    let sha256: String
    let mimeType: String
    let deadLine: Date
    let protection: Int
    let securityPayload: String
}

struct CreateFileEntryResponse: Codable {
    let id: Int64
    let frames: Int
    let nextRequestedFrame: Int
}
