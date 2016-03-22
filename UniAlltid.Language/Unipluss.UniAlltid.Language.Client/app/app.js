(function () {
    "use strict";

    var app = angular.module("app",
                            ["ngRoute", "common.services", "xeditable", 'ui-notification', 'angularUtils.directives.dirPagination']);

    app.config(['$routeProvider', function($routeProvider) {

        $routeProvider
            .when('/translations', {
                templateUrl: 'app/language/languages.html',
                controller: 'LanguagesController',
                controllerAs: 'vm'
            })
            
            .when('/logs', {
                templateUrl: 'app/logs/logs.html',
                controller: 'LogsController',
                controllerAs: 'vm'
            })

            .otherwise('/translations')
        ;

    }]);

    app.run(['editableOptions', '$http', function (editableOptions, $http) {
        editableOptions.theme = 'bs3';
        var accessToken = localStorage.getItem('accessToken');
        if (accessToken !== null && accessToken!=='') {
            $http.defaults.headers.common['X-AccessToken'] = accessToken;
        }
    }]);

    

}());