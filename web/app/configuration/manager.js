//manager.js
//mark.lawrence

(function () {
    var controllerId = 'SettingsController';

    angular.module('app.settings').controller(controllerId, ['$log', '$timeout', 'configurationService', mainController]);

    function mainController(log, $timeout, service) {
        var vm = this;

        vm.config = {};
        vm.save = save;

        activate();

        vm.minRangeSlider = {
            minValue: 10,
            maxValue: 90,
            options: {
                floor: 0,
                ceil: 100,
                step: 1
            }
        };

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
                    log.info('save configuration');
                    log.info(vm.config);
                });
        }
    }
})()