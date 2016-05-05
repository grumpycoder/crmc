//manager.js
//mark.lawrence

(function () {
    var controllerId = 'SettingsController';

    angular.module('app.settings').controller(controllerId, mainController);

    mainController.$inject = ['logger', '$timeout', 'configurationService'];

    function mainController(logger, $timeout, service) {
        var vm = this;
        vm.title = 'Settings';

        vm.config = {};
        var hub = $.connection.wot;

        vm.lastSaved = {};
        vm.save = save;
        vm.undo = undo;

        activate();

        function activate() {
            logger.log(controllerId + ' actived');
            $.connection.hub.start().done(function () {
                logger.info('hub connection successful');
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
                    hub.server.configurationChange(vm.config).then(function () {
                        logger.info('Changes saved and sent to WOT');
                    });
                });
        }

        function undo() {
            vm.config = vm.lastSaved;
        }
    }
})()