(function () {
	"use strict";

	angular
        .module("common.services")
        .factory("languageResource",
				["$resource",
					"appSettings",
						languageResource]);

	function languageResource($resource, appSettings) {
		return $resource(appSettings.serverPath + "/api/languages/:id", null,
		{
		    'update': { method: 'PUT' }
		});
	}
})();