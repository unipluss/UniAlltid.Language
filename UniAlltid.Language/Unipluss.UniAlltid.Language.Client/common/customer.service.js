(function() {
    "use strict";

    angular
        .module("common.services")
        .factory("customerResource",
            ["$resource",
                "appSettings",
                customerResource]);

    function customerResource($resource, appSettings) {
        return $resource(appSettings.serverPath + "/api/languages/customer/:id", null,
        {

        });
    }
})();