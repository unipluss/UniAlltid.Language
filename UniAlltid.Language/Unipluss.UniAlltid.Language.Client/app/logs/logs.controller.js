(function () {
    "use strict";

    angular
        .module("app")
        .controller("LogsController", ["logResource", "Notification", LogsController]);

    function LogsController(logResource, Notification) {
        var vm = this;

        vm.getLogs = function() {
            logResource.query(function(data) {
                vm.logs = data;
            }, function(error) {
                Notification.error('Could not retrieve data from database');
            });
        }

        vm.getLogs();
    }

}());