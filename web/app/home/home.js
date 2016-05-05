(function () {
    'use strict';

    var controllerId = 'HomeController';

    angular.module('app.home').controller(controllerId, mainController);

    mainController.$inject = ['peopleService', 'censorService', 'storage', 'logger'];

    function mainController(peopleService, censorService, storage, logger) {
        var vm = this;
        vm.title = 'Home';

        vm.stat = {};
        vm.people = [];

        activate();

        var censors = [];

        function activate() {
            logger.log(controllerId + ' active');
            censors = JSON.parse(storage.get('censors'));
            if (!censors) {
                censorService.query('')
                        .then(function (data) {
                            storage.set('censors', JSON.stringify(data));
                            censors = JSON.parse(storage.get('censors'));
                        });
            }
            refresh();
        }

        function refresh() {
            peopleService.getCurrentStats()
                .then(function (data) {
                    vm.stat = data;
                });

            peopleService.get()
                .then(function (data) {
                    vm.people = data;
                });
        }
    }
})();