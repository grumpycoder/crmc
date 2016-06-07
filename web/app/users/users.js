(function () {
    'use strict';

    var controllerId = 'UserController';

    angular.module('app.users').controller(controllerId, UserController);

    UserController.$inject = ['logger', 'userService', 'defaults', 'config'];

    function UserController(logger, service, defaults, config) {
        var vm = this;
        vm.title = 'User Manager';
        vm.description = 'Edit and update users';
        var keyCodes = config.keyCodes;

        vm.availableRoles = [];
        vm.clearCreate = clearCreate;
        vm.currentEdit = {};
        vm.isBusy = false;
        vm.lastDeleted = null;
        vm.lastUpdated = null;
        vm.itemToEdit = {};

        vm.user = {
            //userName: null,
            roles: ['user'],
            fullName: '',
            password: defaults.GENERIC_PASSWORD
        };

        var tableStateRef;

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
            getAvailableRoles();
        };

        vm.addItem = function () {
            vm.user.fullName = parseFullName(vm.user.userName);
            vm.user.email = vm.user.userName + defaults.EMAIL_SUFFIX;

            service.create(vm.user)
                .then(function (data) {
                    //TODO: mapping would allow removal of extend method
                    vm.user = angular.extend(vm.user, data);
                    vm.users.unshift(angular.copy(vm.user));
                    logger.success('User ' + vm.user.userName + ' created!');
                    vm.user.userName = null;
                    //TODO: show error user already exists
                });
        };

        vm.cancelEdit = function (id) {
            vm.currentEdit[id] = false;
        };

        vm.deleteItem = function (user) {
            angular.copy(user, vm.lastDeleted = {});
            service.remove(user.id)
                .then(function (data) {
                    var idx = vm.users.indexOf(user);
                    vm.users.splice(idx, 1);
                });
        };

        vm.editItem = function (user) {
            vm.currentEdit[user.id] = true;
            angular.copy(user, vm.itemToEdit = {});
        };

        function getAvailableRoles() {
            service.availableRoles()
                .then(function (data) {
                    vm.availableRoles = data;
                });
        };

        vm.saveItem = function (user) {
            vm.isBusy = true;

            vm.currentEdit[user.id] = false;
            angular.copy(user, vm.lastUpdated = {});
            logger.log('lastUpdated', vm.lastUpdated);
            var roles = [];

            _.forEach(vm.itemToEdit.roles,
                function (role) {
                    roles.push(role.name);
                });
            vm.itemToEdit.roles = roles;

            service.update(vm.itemToEdit)
                .then(function (data) {
                    angular.extend(user, data);
                    logger.success('User ' + data.userName + ' updated!');
                    vm.isBusy = false;
                });
        };

        vm.search = function (tableState) {
            tableStateRef = tableState;
            var searchTerm;

            if (typeof (tableState.search.predicateObject) != 'undefined') {
                searchTerm = tableState.search.predicateObject.searchTerm;
            }

            vm.isBusy = true;
            service.query(searchTerm)
                .then(function (data) {
                    vm.users = data;
                    logger.log('users', vm.users);
                })
                .finally(function () {
                    vm.isBusy = false;
                });
        };

        vm.undoDelete = function () {
            var roles = [];

            _.forEach(vm.lastDeleted.roles, function (role) {
                roles.push(role.name);
                logger.log('role', role.name);
            });
            vm.lastDeleted.roles = roles;
            vm.lastDeleted.password = defaults.GENERIC_PASSWORD;

            service.create(vm.lastDeleted).then(function (data) {
                logger.success('Successfully restored ' + data.userName);
                vm.users.unshift(data);
                vm.lastDeleted = null;
            });
        };

        vm.undoChange = function () {
            vm.isBusy = true;

            service.update(vm.lastUpdated)
                .then(function (data) {
                    angular.forEach(vm.users,
                        function (u, i) {
                            if (u.id === vm.lastUpdated.id) {
                                vm.users[i] = vm.lastUpdated;
                            }
                        });
                    logger.success('Successfully restored ' + data.userName);
                    vm.lastUpdated = null;
                })
                .finally(function () {
                    vm.isBusy = false;
                });
        };

        function clearCreate($event) {
            if ($event.keyCode === keyCodes.esc) {
                vm.user.userName = '';
            };
        };

        function parseFullName(name) {
            var arr = name.split('.');
            var fullname = '';

            _.forEach(arr, function (v) {
                fullname += _.capitalize(v) + ' ';
            });

            return _.trim(fullname);
        };
    };
})();