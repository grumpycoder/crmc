(function () {
    'use strict';

    var controllerId = 'UserController';

    angular.module('app.users').controller(controllerId, ['$log', 'userService', mainController]);

    function mainController(log, service) {
        var vm = this;
        vm.title = 'Users';

        vm.addItem = addItem;
        vm.availableRoles = [];
        vm.cancelEdit = cancelEdit;
        vm.currentEdit = {};
        vm.deleteItem = deleteItem;
        vm.editItem = editItem;
        vm.saveItem = saveItem;
        vm.search = search;

        vm.user = {
            userName: '',
            roles: ['user'],
            fullName: '',
            password: '1P@ssword'
        }

        var tableStateRef;

        activate();

        function activate() {
            log.info(controllerId + ' activated');
            getAvailableRoles();
        }

        function addItem() {
            vm.user.fullName = parseFullName(vm.user.userName);
            vm.user.emailAddress = vm.user.userName + '@splcenter.org';

            service.create(vm.user)
                .then(function (data) {
                    vm.user = data;
                    vm.users.unshift(angular.copy(vm.user));
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

            service.get()
                .then(function (data) {
                    vm.users = data;
                });
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