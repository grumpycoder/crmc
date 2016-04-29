(function () {
    'use strict';

    var controllerId = 'UserController';

    angular.module('app.users').controller(controllerId, ['$log', UserController]);

    function UserController(log) {
        var vm = this;

        vm.title = 'User Manager';

        activate();

        function activate() {
        }
    }
})();