(function() {
    "use strict";

    angular
        .module("app")
        .controller("MainController", ["$http", MainController]);

    function MainController($http) {
        var vm = this;

        vm.hasToken = false;

        vm.getToken = function () {
            vm.accessToken = localStorage.getItem('accessToken');
            if (vm.accessToken !== null) {
                $http.defaults.headers.common['X-AccessToken'] = vm.accessToken;
                vm.hasToken = true;
            }
        }

        vm.setToken = function () {
            localStorage.setItem('accessToken', vm.accessToken);
            vm.getToken();
        }

        vm.getToken();
    }

}());