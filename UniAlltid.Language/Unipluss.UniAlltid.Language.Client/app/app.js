(function () {
    "use strict";

    var app = angular.module("app",
                            ["common.services", "xeditable", 'ui-notification', 'angularUtils.directives.dirPagination']);


    app.run(['editableOptions', '$http', function (editableOptions, $http) {
        editableOptions.theme = 'bs3';
        var accessToken = localStorage.getItem('accessToken');
        if (accessToken !== null && accessToken!=='') {
            $http.defaults.headers.common['X-AccessToken'] = accessToken;
        }
    }]);

    

}());