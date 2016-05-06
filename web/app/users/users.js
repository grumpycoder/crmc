(function () {
    'use strict';

    var controllerId = 'UserController';

    angular.module('app.users').controller(controllerId, mainController);

    mainController.$inject = ['logger', 'userService', 'defaults', 'config'];

    function mainController(logger, service, defaults, config) {
        var vm = this;
        vm.title = 'Users';
        var keyCodes = config.keyCodes;

        vm.addItem = addItem;
        vm.availableRoles = [];
        vm.cancelEdit = cancelEdit;
        vm.clearSearch = clearSearch;
        vm.currentEdit = {};
        vm.deleteItem = deleteItem;
        vm.editItem = editItem;
        vm.saveItem = saveItem;
        vm.search = search;

        vm.user = {
            //userName: null,
            roles: ['user'],
            fullName: '',
            password: defaults.GENERIC_PASSWORD
        }

        var tableStateRef;

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
            getAvailableRoles();
        }

        function addItem() {
            vm.user.fullName = parseFullName(vm.user.userName);
            vm.user.email = vm.user.userName + defaults.EMAIL_SUFFIX;

            service.create(vm.user)
                .then(function (data) {
                    //TODO: mapping would allow removal of extend method
                    vm.user = angular.extend(vm.user, data);
                    vm.users.unshift(angular.copy(vm.user));
                    vm.user.userName = null;
                    //TODO: show error user already exists
                });
        }

        function cancelEdit(id) {
            vm.currentEdit[id] = false;
        }

        function deleteItem(user) {
            service.remove(user.id).then(function (data) {
                var idx = vm.users.indexOf(user);
                vm.users.splice(idx, 1);
            });
        }

        function editItem(user) {
            vm.currentEdit[user.id] = true;
            vm.itemToEdit = angular.copy(user);
        }

        function getAvailableRoles() {
            service.availableRoles()
                .then(function (data) {
                    vm.availableRoles = data;
                });
        }

        function saveItem(user) {
            vm.currentEdit[user.id] = false;
            var roles = [];

            _.forEach(user.roles, function (role) {
                roles.push(role.name);
            });
            user.roles = roles;

            service.update(user)
                .then(function (data) {
                    angular.extend(user, data);
                });
        }

        function search(tableState) {
            tableStateRef = tableState;
            var searchTerm;

            if (typeof (tableState.search.predicateObject) != "undefined") {
                searchTerm = tableState.search.predicateObject.searchTerm;
            }
            service.query(searchTerm)
                .then(function (data) {
                    vm.users = data;
                });
        }

        function clearSearch($event) {
            if ($event.keyCode === keyCodes.esc) {
                tableStateRef.search.predicateObject.searchTerm = undefined;
                search(tableStateRef);
            }
        }

        function parseFullName(name) {
            var arr = name.split('.');
            var fullname = '';

            _.forEach(arr, function (v) {
                fullname += _.capitalize(v) + ' ';
            });

            return _.trim(fullname);
        };
    }
})();