(function () {
    "use strict";

    angular
        .module("common.services")
        .factory("logResource",
            ["$resource",
                "appSettings",
                logResource]);

    function logResource($resource, appSettings) {
        return $resource(appSettings.serverPath + "/api/languages/logs", null,
        {

        });
    }
})();