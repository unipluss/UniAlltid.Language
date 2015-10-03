(function () {
	"use strict";

    angular
        .module("common.services",
					["ngResource"])
        .constant("appSettings",
        {
            serverPath: "https://unilanguageapi.azurewebsites.net"
        });
})();