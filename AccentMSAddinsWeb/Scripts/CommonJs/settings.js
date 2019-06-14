var settings = {
    appName: "Accent Technologies",
    sizeModeThumbnail: {
        extraLarge: "xl",
        large: "lg"
    },
    userAgent: "PowerPoint Add-In (QA build)",
    errorMsgs: {
        wrongUsrPass: "Invalid username or password.",
        wrongLib: "Failled to execute request.",
    }
}

var urls = {
    serviceUrl: window.location.origin + "/api/",
    middlewareUrl: "https://127.0.0.1:9999/api/",
    middlewareDomain: "https://127.0.0.1:9999/",
    getccUrl: "/Default.ashx?m=getcc&cid={catId}&useweb=false&out=json",
    getWebClientUrl: "https://www.accent-technologies.com/libraries/lookup.aspx?out=json&email={username}&ename={environmentName}&lname={libraryName}",
    downloadUrl: "/Download.ashx?mode=dnld&fid={fileId}&useweb=false?out=xml",
    deleteUrl: "/Update.ashx?m=fdel&fid={fileId}&cid={catId}&out=json",
    getThumbnail: "/Download.ashx?mode=thmb&tname={sizeMode}&fid={fileId}&snum={slideNumber}&useweb=false?out=xml",
    searchFilesUrl: "/Search.ashx?out=json",
}