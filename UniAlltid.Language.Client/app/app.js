(function () {
    "use strict";

    var app = angular.module("app",
                            ["common.services", "xeditable", 'ui-notification', 'angularUtils.directives.dirPagination']);


    app.run(function (editableOptions) {
        editableOptions.theme = 'bs3'; // bootstrap3 theme. Can be also 'bs2', 'default'
    });

    app

}());