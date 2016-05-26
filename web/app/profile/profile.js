//mark.lawrence
//profile.js

(function () {
    'use strict';

    var controllerId = 'ProfileController';

    angular.module('app.users').controller(controllerId, mainController);

    mainController.$inject = ['logger', 'userService', 'defaults', 'config'];

    function mainController(logger, service, defaults, config) {
        var vm = this;
        vm.title = 'Profile Manager';
        vm.description = 'Update your profile';

        vm.user = {};

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
            getUserData();
        }

        function getUserData() {
            service.query('mark.lawrence')
                .then(function (data) {
                    vm.user = data[0];
                    logger.log(vm.user);
                });
        }

        vm.save = function () {
            vm.isBusy = true;
            logger.log('user', vm.user);
            service.update(vm.user)
                .then(function (data) {
                    vm.user = data;
                    logger.info(data);
                }).finally(function () {
                    vm.isBusy = false;
                });
        }
    }
})();