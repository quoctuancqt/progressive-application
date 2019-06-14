var officeHelper = {
    getDocumentAsCompressed: function (callback) {
        Office.context.document.getFileAsync(Office.FileType.Compressed, { sliceSize: 65536 /*64 KB*/ },
            function (result) {
                if (result.status == "succeeded") {
                    var myFile = result.value;
                    var sliceCount = myFile.sliceCount;
                    var slicesReceived = 0, gotAllSlices = true, docdataSlices = [];

                    officeHelper.getSliceAsync(myFile, 0, sliceCount, gotAllSlices, docdataSlices, slicesReceived, callback);
                }
                else {
                    return;
                }
            });
    },
    getSliceAsync: function (file, nextSlice, sliceCount, gotAllSlices, docdataSlices, slicesReceived, callback) {
        file.getSliceAsync(nextSlice, function (sliceResult) {
            if (sliceResult.status == "succeeded") {
                if (!gotAllSlices) {
                    return;
                }

                docdataSlices[sliceResult.value.index] = sliceResult.value.data;

                if (++slicesReceived == sliceCount) {
                    file.closeAsync();
                    officeHelper.onGotAllSlices(docdataSlices, callback);
                }
                else {
                    getSliceAsync(file, ++nextSlice, sliceCount, gotAllSlices, docdataSlices, slicesReceived);
                }
            }
            else {
                gotAllSlices = false;
                file.closeAsync();
            }
        });
    },
    onGotAllSlices: function (docdataSlices, callback) {
        var docdata = [];
        for (var i = 0; i < docdataSlices.length; i++) {
            docdata = docdata.concat(docdataSlices[i]);
        }

        var fileContent = new String();
        for (var j = 0; j < docdata.length; j++) {
            fileContent += String.fromCharCode(docdata[j]);
        }

        if (callback) {
            callback(docdata);
        }
    }
}