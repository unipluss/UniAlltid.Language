(function () {
    "use strict";
    angular
        .module("app")
        .controller("LanguagesController", ["languageResource", "customerResource", "Notification", LanguagesController]);

    function LanguagesController(languageResource, customerResource, Notification) {
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

        vm.updateKeyId = function(language) {
            language.$updateKey(function(data) {
                Notification.success('Database updated!');
                vm.reloadData();
            }, function(error) {
                Notification.error('Could not update key in database');
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

        vm.deleteValue = function(id) {
            var language = new languageResource();

            language.$delete({ id: id }, function (data) {
                Notification.success('Deleted from database!');
                vm.reloadData();
            }, function(error) {
                Notification.error('Could not delete entry from database');
            });
        }

        vm.getCustomers = function () {
            customerResource.query(function (data) {
                vm.customers = data;
            });
        }

        vm.createCustomer = function() {
            var customer = new customerResource();

            customer.id = vm.newCustomerId;
            customer.name = vm.newCustomerName;

            customer.$save(function(data) {
                Notification.success('Added to database!');
                vm.getCustomers();
                vm.newCustomerId = '';
                vm.newCustomerName = '';
            }, function(error) {
                Notification.error('Could not add customer to database');
            });
        }

        vm.reloadData();
        vm.getCustomers();
    }
}());