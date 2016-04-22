//manager.js
//mark.lawrence

(function () {
    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, ['$log', 'censorService', mainController]);

    function mainController(log, service) {
        var vm = this;

        vm.editItem = editItem;
        vm.cancelEdit = cancelEdit;
        vm.deleteItem = deleteItem;
        vm.saveItem = saveItem;

        vm.censors = [];
        vm.currentEdit = {};
        vm.searchTerm = '';
        vm.search = search;

        activate();

        function activate() {
            log.info('Censor Controller');
            search();
        }

        function search() {
            service.query(vm.searchTerm).then(function (data) {
                vm.censors = data;
            });
        }

        function cancelEdit(id) {
            vm.currentEdit[id] = false;
        }

        function editItem(censor) {
            log.info('edit');
            log.info(censor);
            vm.currentEdit[censor.id] = true;
            vm.itemToEdit = angular.copy(censor);
        }

        function deleteItem(censor) {
            service.remove(censor.id)
                .then(function (data) {
                    var idx = vm.censors.indexOf(censor);
                    vm.censors.splice(idx, 1);
                });
        }

        function saveItem(censor) {
            vm.currentEdit[censor.id] = false;
            service.update(censor).then(function (data) {
                censor = data;
            });
        }
    }
})()