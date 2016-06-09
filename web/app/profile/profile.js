//mark.lawrence
//profile.js

(function () {
    'use strict';

    var controllerId = 'ProfileController';

    angular.module('app.users').controller(controllerId, mainController);

    mainController.$inject = ['$scope', '$http', 'logger', 'userService', 'defaults', 'config', '$rootScope'];

    function mainController($scope, $http, logger, service, defaults, config, $rootScope) {
        var vm = this;
        vm.title = 'Profile Manager';
        vm.description = 'Update your profile';

        vm.user = {};

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
            vm.user = JSON.parse(localStorage.getItem('currentUser'));
        };

        vm.fileSelected = function ($files, $file, $event, $rejectedFiles) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var dataURL = reader.result;
                vm.user.userPhoto = dataURL.split(',')[1];
                vm.user.userPhotoType = $file.type;
            };
            reader.readAsDataURL($file);
        };

        vm.save = function () {
            vm.isBusy = true;
            service.update(vm.user)
                .then(function (data) {
                    vm.user = data;
                    localStorage.setItem('currentUser', JSON.stringify(vm.user));
                })
                .finally(function () {
                    vm.isBusy = false;
                    vm.file = null;
                    vm.profileForm.$setPristine();
                    vm.avatarForm.$setPristine();
                });
        };
    };
})();