(function () {
    "use strict";
    angular
        .module("app")
        .controller("LanguagesController", ["languageResource","Notification",LanguagesController]);

    function LanguagesController(languageResource,Notification) {
        var vm = this;

        vm.predicate = 'keyId';
        vm.reverse = false;
        vm.showAddNew = false;

        vm.order = function(predicate) {
        	vm.reverse = (vm.predicate === predicate) ? !vm.reverse : false;
        	vm.predicate = predicate;
        }

        vm.reloadData = function() {
        	languageResource.query({ customer: vm.customer, language: vm.language }, function (data) {
        		vm.languages = data;
        	}, function(error) {
        		Notification.error('Could not retrieve data from database');
	        });
        }

        vm.updateValue = function (language) {

        	language.$update({ selectedCustomer: vm.customer }, function (data) {
        		Notification.success('Database updated!');
        		vm.reloadData();
	        }, function(error) {
			    Notification.error('Could not update entry in database');
	        });
        }

        vm.createValue = function () {

        	var language = new languageResource();

            language.keyId = vm.newKeyId;
            language.value = vm.newValue;
            language.valueEnglish = vm.newValueEnglish;

            language.$save(function (data) {
            	Notification.success('Added to database!');
            	vm.reloadData();
            	vm.newKeyId = '';
            	vm.newValue = '';
                vm.newValueEnglish = '';
            }, function(error) {
                Notification.error('Could not add entry to database');
            });
        }

        vm.reloadData();
    }
}());