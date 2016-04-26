//manager.js
//mark.lawrence

(function () {
    var controllerId = 'SettingsController';

    angular.module('app.settings').controller(controllerId, ['$log', 'configurationService', mainController]);

    function mainController(log, service) {
        var vm = this;

        vm.config = {};
        vm.save = save;

        activate();

        function activate() {
            log.info('settings controller active');
            getSettings();
        }

        function getSettings() {
            service.get().then(function (data) {
                vm.config = data;
                log.info(vm.config);
            });
        }

        function save() {
            service.update(vm.config)
                .then(function (data) {
                    vm.config = data;
                });
        }
    }
})()