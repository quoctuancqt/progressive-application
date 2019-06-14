"use strict";

var authHash = storageLibrary.getFromLocalStorage("authHash");
var webClientUrl = storageLibrary.getFromLocalStorage("webClientUrl");
var libraryShortName = storageLibrary.getFromLocalStorage("libraryShortName");
var environmentName = storageLibrary.getFromLocalStorage("environmentName");
var timeout = false;
var data = [];
var selectedNodes = [];
var nodeData = [];
var selectedFile = {};
var dialogComponent;
var currentNodeId = 0;
var dbClickFile = false;

Office.initialize = function (reason) {
    $(document).ready(function () {
        dialogComponent = loadingIndicator.register();

        loadingIndicator.show();

        initGrid();

        registerEvents();

        setTimeout(function () {
            buildTree([0, 101, 4977, 4980], function (resp) {
                initTree(resp);
                loadingIndicator.hide();
            });
        }, 500);

        $("#fileLocation").click(function () {
            $("#downloadResultContainer").hide();
        });
    });
};

function initGrid() {
    $('#grid').w2grid({
        name: 'grid',
        columns: [
            { field: 'recid' },
            { field: 'fileName' },
            { field: 'text', caption: 'Name', size: '30%', resizable: true, sortable: true },
            { field: 'date', caption: 'Date modified', size: '30%', resizable: true, sortable: true },
            { field: 'size', caption: 'Size', size: '40%', resizable: true, sortable: true },
        ]
    });

    w2ui['grid'].hideColumn('recid');
    w2ui['grid'].hideColumn('fileName');

    w2ui.grid.on('dblClick', function (event) {
        if (event.recid != currentNodeId) {
            $("#jstree").jstree(true).activate_node([event.recid]);
        }
        $.each(w2ui.grid.records, function (i, v) {
            if (v.recid == event.recid) {
                if (v.fileType == 'file') {
                    var f = {
                        id: v.recid,
                        text: v.fileName
                    };
                    onOpen(f);
                }
            }
        })
    });

    w2ui.grid.on('click', function (event) {
        $("#downloadResultContainer").hide();
        if (event.recid != currentNodeId) {
            var selectedNode = $("#jstree").jstree(true).get_node(event.recid);
            if (selectedNode) {
                if (selectedNode.type == "file") {
                    $("#jstree").jstree(true).activate_node([event.recid]);
                }
            }
            else {
                var fileInfo = $.grep(w2ui.grid.records, function (e) {
                    return e.recid == event.recid;
                });
                var fileName = fileInfo[0].text.substring(45, fileInfo[0].text.length);

                selectedFile = {
                    id: fileInfo[0].recid,
                    text: fileName,
                    parent: currentNodeId
                };

                getThumbnail(selectedFile, 0);
                showHideActionButton("file");
            }
        }
    });
}

function initTree(resp) {
    prepareData(resp);

    nodeContent(data[0]);

    $('#jstree').jstree({
        "plugins": ["types", "state", "search"],
        "core": {
            "data": data,
            "check_callback": true
        },
        "types": {
            "default": {
                "icon": "/Images/folder-icon.png"
            },
            "file": {
                "icon": "ms-Icon ms-Icon--PowerPointLogo"
            }
        }
    });

    $('#jstree').jstree(true).clear_state();

    $('#jstree').on("select_node.jstree", handleSelectNode);

    $("#jstree").on("ready.jstree", function (e, d) {
        d.instance.open_node(["0"]);
        d.instance.select_node(["0"]);
    });
}

function refreshTree() {
    selectedNodes = [];
    var selectedNode = $("#jstree").jstree(true).get_node(currentNodeId);

    if (selectedNode.type == "file") {
        selectedNode = $("#jstree").jstree(true).get_node(selectedNode.parent);
    }
    var children = selectedNode.children;
    var isRefresh = children.length > 0 ? true : false;

    loadingIndicator.show();

    try {
        $("#jstree").jstree(true).delete_node(children);
    }
    catch (err) {
        selectedNode.children = [];

        $.each(children, function (i, v) {
            remove(data[0], function (element) {
                return element.id && element.id == v;
            });
        });

        $('#jstree').jstree(true).settings.core.data = data;
    }

    if (isRefresh) {
        getData(currentNodeId, function (resp) {
            buildSubTree(selectedNode, resp);
            if (selectedNode.children.length > 0) {
                nodeContent(selectedNode);
            }
        });
    }

    if (selectedNodes.indexOf(currentNodeId) == -1) {
        selectedNodes.push(currentNodeId);
    }

    $("#jstree").jstree(true).activate_node([selectedNode.id]);

    loadingIndicator.hide();
}

function handleSelectNode(e, node) {
    var selected = node.instance.get_selected()[0];
    var selectedNode = node.instance.get_node(selected);
    if (selected != currentNodeId) {
        showHidePreview(false);
        currentNodeId = selected;
        if (selected != 0) {
            if (selectedNode.type == "file") {
                $("#downloadResultContainer").hide();
                selectedFile = selectedNode;
                getThumbnail(selectedFile, 0);
            }
            if (selectedNodes.indexOf(selected) == -1) {
                selectedNodes.push(selected);
                if (selectedNode.type == "file") {
                    $('#jstree').jstree(true).open_node(selectedNode);
                }
                else {
                    loadingIndicator.show();
                    showHidePreview(false);
                    getData(selected, function (resp) {
                        buildSubTree(selectedNode, resp);
                        if (selectedNode.children.length > 0) {
                            nodeContent(selectedNode);
                        }
                        loadingIndicator.hide();
                    }, null, true);
                }
            }

            showHideActionButton(selectedNode.type);
        }
        else {
            nodeContent(data[0]);
        }
    }
    else {
        $('#jstree').jstree(true).open_node(selectedNode);
    }
}

function prepareData(resp) {
    data = [];
    selectedNodes = [];

    $.each(resp, function (i, v) {
        if (v.Name == "Approved") {
            v.Name = "My Creations";
        }
        if (v.Type == 2) {
            v.Name = "My Files";
        }
        data.push({
            id: v.CatId.toString(),
            text: v.Name,
            children: [],
        });

        if (v.SubCategories.length > 0) {
            $.each(v.SubCategories, function (j, vv) {
                var item = {
                    id: vv.CatId,
                    text: vv.Name,
                    children: []
                };

                data[i].children.push(item);
            })
        }
    })
}

function buildTree(arrayRootId, callback) {
    var initData = [];

    for (var i = 0; i < arrayRootId.length; i++) {
        getData(arrayRootId[i], function (resp) {
            initData.push(resp);
        }, function (error) {
            console.log(error);
        }, false);
    }

    callback(initData);
}

function buildSubTree(selectedNode, resp) {
    if (resp.SubCategories.length > 0) {
        $.each(resp.SubCategories, function (i, v) {
            var item = {
                id: v.CatId,
                text: v.Name,
                children: []
            };
            selectedNode.children.push(item);
        })
    }

    if (resp.Files.length > 0) {
        $.each(resp.Files, function (i, v) {
            if (v.FileType == 48) {
                var file = {
                    id: v.FileId,
                    text: v.Name,
                    type: "file",
                    date: moment(v.LastModDate).format("DD-MM-YYYY, hh:mm:ss"),
                    size: v.FileSize + "Kb",
                    children: []
                }
                selectedNode.children.push(file);
            }
        })
    }

    updateData(data, selectedNode);

    $('#jstree').jstree(true).settings.core.data = data;
    $('#jstree').jstree(true).refresh(selectedNode);
}

function getData(parentId, callbackSuccess, callbackError, isAsync) {
    var endpoint = webClientUrl + libraryShortName + urls.getccUrl;

    $.ajax({
        method: "GET",
        async: isAsync,
        headers: {
            'Authorization': authHash
        },
        url: endpoint.replace("{catId}", parentId),
        success: function (resp) {
            if (callbackSuccess) {
                callbackSuccess(resp);
            };
        },
        error: function (error) {
            if (callbackError) {
                callbackError(error);
            }
        }
    });
}

function updateData(d, selectedNode) {
    $.each(d, function (i, v) {
        if (v.id == selectedNode.id) {
            v.children = selectedNode.children;
        }
        else {
            if (v.children.length > 0) {
                updateData(v.children, selectedNode);
            }
        }
    });
}

function nodeContent(selectedNode) {
    var lstRecords = {
        total: 0,
        records: []
    };
    if (selectedNode) {
        if (selectedNode.children.length > 0) {
            lstRecords.total = selectedNode.children.length;

            $.each(selectedNode.children, function (i, v) {
                var item = {
                    recid: v.id,
                    text: '<img src="/Images/folder-icon.png" /> ' + ' ' + v.text,
                    fileName: v.text,
                    fileType: 'folder',
                    date: v.date,
                    size: v.size
                };

                if (v.type == 'file') {
                    item.text = '<i class=\"ms-Icon ms-Icon--PowerPointLogo\"> ' + ' ' + v.text;
                    item.fileType = 'file';
                }

                lstRecords.records.push(item);
            })
        }
        else {
            var item = {
                recid: selectedNode.id,
                text: '<img src="/Images/folder-icon.png" /> ' + ' ' + selectedNode.text,
                fileName: selectedNode.text,
                fileType: '',
                date: selectedNode.date,
                size: selectedNode.size
            };

            if (selectedNode.type == 'file') {
                item.text = '<i class=\"ms-Icon ms-Icon--PowerPointLogo\"> ' + ' ' + selectedNode.text;
                item.fileType = 'file';
            }

            lstRecords.records.push(item);
        }
    }
    else {
        lstRecords.records = [];
    }

    w2ui.grid.records = lstRecords.records;
    w2ui.grid.refresh();
}

function registerEvents() {
    $("#btn-close").click(function () {
        Office.context.ui.messageParent("closeDialog");
    });

    $("#btn-refresh").click(function () {
        refreshTree();
    });

    $("#btn-download").click(function () {
        if (!$(this).parent().hasClass("is-disabled")) {
            onDownload(selectedFile);
        }
    })

    $("#btn-open").click(function () {
        if (!$(this).parent().hasClass("is-disabled")) {
            onOpen(selectedFile);
        }
    });

    $("#btn-delete").click(function () {
        if (!$(this).parent().hasClass("is-disabled")) {
            var msg = "About to delete: " + selectedFile.text + ".";
            $("#confirm-content").text(msg);
            $("#overlay").addClass("dark-class");
            $("#confirm-dialog").show();
        }
    });

    $("#searchKey").keyup(function (e) {
        if (e.keyCode == 13) {
            var searchValue = $('#searchKey').val();
            search(currentNodeId, searchValue);
        }
        else {
            return;
        }
    });

    $("#clearSearch").click(function () {
        $("#searchKey").val('');
        $("#jstree").jstree(true).clear_search();
    });

    //$("#downloadDefaultLocation").click(function () {
    //    if (!$(this).parent().hasClass("is-disabled")) {
    //        onDownload(selectedFile);
    //    }
    //});
}

function onDownload(file) {
    loadingIndicator.show();

    var pData = {
        ServerInfo: {
            WebClientUrl: webClientUrl,
            ShortLibraryName: libraryShortName,
            AuthHash: authHash,
            EnvironmentName: environmentName
        },
        FileId: file.id,
        FileName: file.text,
        FileUrl: "",
        AskFirst: false,
        IsConfirm: false,
        IsOverwrite: false
    };

    var options = {
        method: "POST",
        postData: JSON.stringify(pData),
        contentType: "application/json"
    };

    xhrHelper.makeCorsRequest(urls.middlewareUrl + "files/getfile", options, function (response) {
        loadingIndicator.hide();
        $("#downloadResultContainer").show();
        $("#fileLocation").attr("href", response.srcElement.response);
        $("#fileLocation").text(response.srcElement.response);
    }, function (error) {
        loadingIndicator.hide();
    });
}

function onOpen(file) {
    loadingIndicator.show();
    var endpoint = urls.middlewareUrl + "files/openfile?webClientUrl={webClientUrl}&shortLibraryName={shortLibraryName}&authHash={authHash}&fileId={fileId}&fileName={fileName}";
    endpoint = endpoint.replace("{webClientUrl}", webClientUrl);
    endpoint = endpoint.replace("{shortLibraryName}", libraryShortName);
    endpoint = endpoint.replace("{authHash}", authHash);
    endpoint = endpoint.replace("{fileId}", file.id);
    endpoint = endpoint.replace("{fileName}", commonHelper.removeSpecialChars(file.text));
    var link = document.getElementById("linkOpenFile");
    link.href = endpoint;
    link.click();
    loadingIndicator.hide();
}

function onDelete(file) {
    $("#processMsg").show();
    var deleteNode = $('#jstree').jstree(true).get_node(file.id);

    var endpoint = webClientUrl + libraryShortName + urls.deleteUrl;
    endpoint = endpoint.replace("{fileId}", file.id).replace("{catId}", file.parent);

    var options = {
        method: "GET",
        token: authHash
    };

    xhrHelper.makeCorsRequest(endpoint, options, function (response) {
        $("#processMsg").hide();
        $("#confirm-dialog").hide();
        $("#overlay").removeClass("dark-class");
        refreshTree();
    }, function (error) {
        $("#confirm-dialog").hide();
        $("#overlay").removeClass("dark-class");
    });
}

function getThumbnail(file, slideNumber) {
    loadingIndicator.show();
    var endpoint = urls.middlewareUrl + "files/getthumbnail?webClienUrl={webClientUrl}&shortLibraryName={shortLibraryName}&authHash={authHash}&sizeMode={sizeMode}&fileId={fileId}&slideNumber={slideNumber}";
    endpoint = endpoint.replace("{webClientUrl}", webClientUrl);
    endpoint = endpoint.replace("{shortLibraryName}", libraryShortName);
    endpoint = endpoint.replace("{authHash}", authHash);
    endpoint = endpoint.replace("{sizeMode}", settings.sizeModeThumbnail.large);
    endpoint = endpoint.replace("{fileId}", file.id);
    endpoint = endpoint.replace("{slideNumber}", slideNumber);

    var options = {
        method: "GET"
    };

    xhrHelper.makeCorsRequest(endpoint, options, function (response) {
        showHidePreview(true);
        $("#thumbnail").attr("src", commonHelper.getBase64DataResponse(response));
        loadingIndicator.hide();
    }, function (error) {
        loadingIndicator.hide();
    });
}

function handleSuccess(filename, data) {
    if (typeof window.navigator.msSaveBlob !== 'undefined') {
        var blob = commonHelper.base64ToBlob(data);
        window.navigator.msSaveOrOpenBlob(blob, filename);
    } else {
        var a = document.createElement("a");
        a.href = data;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
    }
}

function search(currentNodeId, searchKey) {
    loadingIndicator.show();
    var data = buildSearchTerm(currentNodeId, searchKey);

    var endpoint = webClientUrl + libraryShortName + urls.searchFilesUrl;

    var options = {
        method: "POST",
        postData: data,
        token: authHash
    };

    xhrHelper.makeCorsRequest(endpoint, options, function (resp) {
        loadingIndicator.hide();
        var jsonObj = JSON.parse(resp.srcElement.response);
        displaySearchResult(jsonObj.Items);
    }, function (err) {
        loadingIndicator.hide();
    });
}

function buildSearchTerm(currentNodeId, searchKey) {
    var result = "";

    $.ajax({
        type: "GET",
        url: "/Content/xml/search.xml",
        dataType: "xml",
        async: false,
        success: function (xml) {
            var xmlText = new XMLSerializer().serializeToString(xml);;
            xmlText = xmlText.replace(/{searchKey}/g, searchKey).replace("{catId}", currentNodeId);
            result = cypherHelper.string2base(xmlText);
        },
        error: function (err) {
            console.log(err);
        }
    });

    return result;
}

function displaySearchResult(d) {
    var lstRecords = {
        total: d.length,
        records: []
    };

    $.each(d, function (i, v) {
        var item = {
            recid: v.FileInfo.FileId,
            text: '<i class=\"ms-Icon ms-Icon--PowerPointLogo\"> ' + ' ' + v.FileInfo.Name,
            date: v.FileInfo.LastModDate,
            size: v.FileInfo.FileSize
        };

        lstRecords.records.push(item);
    });

    w2ui.grid.records = lstRecords.records;
    w2ui.grid.refresh();
}

//helper
function showHidePreview(isFile) {
    if (isFile) {
        $("#thumbnail").show();
        $("#noThumbnail").hide();
    }
    else {
        $("#thumbnail").hide();
        $("#noThumbnail").show();
    }
}

function showHideActionButton(nodeType) {
    if (nodeType == "file") {
        commonHelper.removeClass("#btn-open", "custom-disabled", true);
        commonHelper.removeClass("#btn-download", "custom-disabled", true);
        commonHelper.removeClass("#btn-delete", "custom-disabled", true);
        commonHelper.removeClass(".ms-CommandButton-splitIcon", "custom-disabled", false);
    }
    else {
        commonHelper.addClass("#btn-open", "custom-disabled", true);
        commonHelper.addClass("#btn-download", "custom-disabled", true);
        commonHelper.addClass("#btn-delete", "custom-disabled", true);
        commonHelper.addClass(".ms-CommandButton-splitIcon", "custom-disabled", false);
    }
}

function findNodeData(ids, d) {
    for (var i = ids.length - 1; i >= 0; i--) {
        $.each(d, function (j, v) {
            if (v.id == ids[i]) {
                var k = i - 1;
                if (v.children.length > 0 && k >= 0) {
                    ids.splice(i, 1);
                    findNodeData(ids, v.children);
                }
                else {
                    ids.splice(i, 1);
                }
            }
        });
        break;
    }
}

function remove(src, predicate) {

    // for Array
    if (Array.isArray(src)) {
        for (var i = src.length - 1; i > -1; i--) {
            if (predicate(src[i])) {
                src.splice(i, 1);
            } else {
                remove(src[i], predicate);
            }
        }
    }

    // for Object
    else {
        for (var i in src) {
            if (i == "children") {
                if (predicate(src[i])) {
                    delete src[i];
                } else {
                    remove(src[i], predicate);
                }
            }
        }
    }
}