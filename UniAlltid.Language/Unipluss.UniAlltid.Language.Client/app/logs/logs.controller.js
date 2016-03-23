(function () {
    "use strict";

    angular
        .module("app")
        .controller("LogsController", ["logResource", "Notification", LogsController]);

    function LogsController(logResource, Notification) {
        var vm = this;

        vm.getLogs = function () {
            vm.loading = true;

            logResource.query(function(data) {
                vm.logs = data;
                vm.loading = false;
            }, function(error) {
                Notification.error('Could not retrieve data from database');
                vm.loading = false;
            });
        }

        vm.getLogs();
    }

}());