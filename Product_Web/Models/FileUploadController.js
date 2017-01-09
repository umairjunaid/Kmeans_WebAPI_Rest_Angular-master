app.controller('FileUploadController', function ($scope) {
    //mageUploadMultipleCtrl is our controller name
    $scope.fileList = [];//$scope variable fileList is an array of files that you selected
    $scope.ImageProperty = {
        file: ''
    }

    $scope.setFile = function (element) {
        $scope.fileList = [];
        // get the files
        var files = element.files;
        for (var i = 0; i < files.length; i++) {
            $scope.ImageProperty.file = files[i];

            $scope.fileList.push($scope.ImageProperty);
            $scope.ImageProperty = {};
            $scope.$apply();

        }
     
    }
//UploadFile() -- this function call when we click upload button. 
//UploadFileIndividual(fileToUpload,name,type,size,index) -- responsible for upload indivudual file. 
//It takes 4 parameters: the file to upload,the file name,the file type ,file size and index 
    $scope.UploadFile = function () {

 
            $scope.UploadFileIndividual($scope.fileList[0].file,
                                        $scope.fileList[0].file.name,
                                        0);
        }

 
    $scope.UploadFileIndividual = function (fileToUpload, name, type, size, index) {
        //Create XMLHttpRequest Object
        var reqObj = new XMLHttpRequest();
        //open the object and set method of call(get/post), url to call, isasynchronous(true/False)
        reqObj.open("POST", "/FileUpload/UploadFile", true);
        //set Content-Type at request header.For file upload it's value must be multipart/form-data
        reqObj.setRequestHeader("Content-Type", "multipart/form-data");
        //Set Other header like file name,size and type
        reqObj.setRequestHeader('X-File-Name', name);
        // send the file
        reqObj.send(fileToUpload);
    }

});