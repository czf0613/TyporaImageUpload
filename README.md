# Typora Image Upload
Typora is a very useful markdown editor, and now we have a free image uploader for Typora using the "KCos Project", which is invented by ourselves.

## Free

It is free to use this upload tool because we want more people to know us.

But, do not use this free version for any business purpose, which cannot guarantee your full support and data security.

## Global CDN

KCos is using high quality CDN provided by the AliCloud. That can ensure a low latency wherever you are.

## Cross Platform

This upload tool is built for many popular platforms statically, which means that you can use it without any external libraries. (That's why the binary file seems large) Most of the platforms are supported by Dotnet Runtime, except Apple Devices. 

So, in this project, you can find out the Dotnet version in the Dotnet Folder. By the way, a pure Swift implementation is also provided in the Swift Folder.

Supported platforms are as follows:

| Platform                               | Supporting | Build Pass                                        |
| -------------------------------------- | ---------- | ------------------------------------------------- |
| Early Windows(Like 8.1, 7, Vista, XP)  | Nope       | Nope                                              |
| any 32 bit platforms(Like arm v7, x86) | Nope       | Nope                                              |
| Early macOS(before 11)                 | Nope       | Nope. It does not support Swift byte code.        |
| macOS(after 11) x64 arch               | Yes        | Yes                                               |
| macOS Apple Silicon                    | Yes        | Yes                                               |
| many-linux-arm64                       | Yes        | Yes                                               |
| many-linux-x64                         | Yes        | Yes                                               |
| Modern Windows(10 or later) x64 arch   | Yes        | Yes                                               |
| Windows 10 on arm                      | Nope       | Does not support x64 simulation.                  |
| Windows 11 arm64 arch                  | Yes        | Yes, it is native build, not the x64 translation. |
| Windows Server x64                     | Yes        | Yes                                               |



# How to use?

## Windows

Download the binary file accroding to your system version.

Put it into your `C:/Programe Files/KCos/` folder. If you don't have this folder, create a new one.

Then, add this path to your system PATH environment variable:

![image-20221106183059630](https://cos.kevinc.ltd/file/download?fileId=24)

Then, rename your binary file like this(remove the arch-bits suffix):

![image-20221106183213206](https://cos.kevinc.ltd/file/download?fileId=25)

Now, have a try, open a terminal and then type this `KCosUpload`:

![image-20221106183310185](https://cos.kevinc.ltd/file/download?fileId=26)

If you receive the Success Info, that means you successfully installed our upload tool.

Then, open the Typora, click the menu button, and follow my navigation:

![image-20221106183511761](https://cos.kevinc.ltd/file/download?fileId=27)

![image-20221106183605269](https://cos.kevinc.ltd/file/download?fileId=28)

The last step, click the `Test Upload` Button, if you receive the passing message, that means you can use our tool in Typora.

![image-20221106183739966](https://cos.kevinc.ltd/file/download?fileId=31)

## macOS & Linux

Usage in macOS and Linux are similar.

First of all, download the right version for you, and put it at any where you like, for example ~/Desktop/KCosUpload.

Then `chmod +x $YOUR_FILE_PATH`.

Then open the Typora, just follow the steps mentioned in `Windows` Section, and you can upload your pictures.
