//mark.lawrence
//profile.js

(function () {
    'use strict';

    var controllerId = 'ProfileController';

    angular.module('app.users').controller(controllerId, mainController);

    mainController.$inject = ['$scope', '$http', 'logger', 'userService', 'defaults', 'config'];

    function mainController($scope, $http, logger, service, defaults, config) {
        var vm = this;
        vm.title = 'Profile Manager';
        vm.description = 'Update your profile';

        vm.user = {};

        activate();

        $scope.$on('flow::fileAdded', function (event, $flow, flowFile) {
            event.preventDefault();//prevent file from uploading
        });

        function activate() {
            logger.log(controllerId + ' activated');
            getUserData();
        };

        function getUserData() {
            //TODO: Need to get logged in username

            service.query('mark.lawrence')
                .then(function (data) {
                    vm.user = data[0];
                });
        };

        vm.save = function () {
            vm.isBusy = true;
            logger.log('user', vm.user);
            service.update(vm.user)
                .then(function (data) {
                    vm.user = data;
                    logger.info(data);
                })
                .finally(function () {
                    vm.isBusy = false;
                    vm.profileForm.$setPristine();
                });
        };

        vm.saveAvatar = function () {
            vm.isAvatarBusy = true;
            service.uploadAvatar(vm.user.userName, vm.file)
                .then(function (data) {
                    logger.success(data);
                    vm.avatarForm.$setPristine();
                    vm.isAvatarBusy = false;
                });
        };
    };
})();