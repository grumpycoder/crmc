//manager.js
//mark.lawrence

(function () {
    var controllerId = 'SettingsController';

    angular.module('app.settings').controller(controllerId, ['$log', '$timeout', 'configurationService', mainController]);

    function mainController(log, $timeout, service) {
        var vm = this;
        vm.title = 'Settings';

        vm.config = {};
        var hub = $.connection.wot;

        vm.lastSaved = {};
        vm.save = save;
        vm.undo = undo;

        activate();

        function activate() {
            log.info(controllerId + ' actived');
            $.connection.hub.start().done(function () {
                log.info('hub connection successful');
            });
            getSettings();
        }

        function getSettings() {
            service.get().then(function (data) {
                vm.config = data;
                vm.lastSaved = angular.copy(vm.config);
            });
        }

        function save() {
            service.update(vm.config)
                .then(function (data) {
                    vm.config = data;
                    hub.server.configurationChange(vm.config);
                });
        }

        function undo() {
            vm.config = vm.lastSaved;
        }
    }
})()