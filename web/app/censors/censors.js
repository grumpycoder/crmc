//manager.js
//mark.lawrence

(function () {
    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, mainController);

    mainController.$inject = ['$log', '$uibModal', 'censorService'];

    function mainController(log, $modal, service) {
        var vm = this;
        vm.title = 'Censors';

        vm.cancelEdit = cancelEdit;
        vm.create = create;
        vm.editItem = editItem;
        vm.deleteItem = deleteItem;
        vm.saveItem = saveItem;

        vm.censors = [];
        vm.currentEdit = {};
        vm.search = search;

        activate();

        function activate() {
            log.info(controllerId + ' activated');
        }

        function search(tableState) {
            var searchTerm = '';
            if (typeof (tableState.search.predicateObject) != "undefined") {
                searchTerm = tableState.search.predicateObject.searchTerm;
            }
            service.query(searchTerm).then(function (data) {
                vm.censors = data;
            });
        }

        function cancelEdit(id) {
            vm.currentEdit[id] = false;
        }

        function create() {
            var item = {};
            $modal.open({
                templateUrl: '/app/censors/views/censor.html',
                controller: ['$uibModalInstance', 'censorService', 'item', createCensorController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            }).result.then(function (data) {
                vm.censors.unshift(data);
            });
        }

        function deleteItem(censor) {
            service.remove(censor.id)
                .then(function (data) {
                    var idx = vm.censors.indexOf(censor);
                    vm.censors.splice(idx, 1);
                });
        }

        function editItem(censor) {
            vm.currentEdit[censor.id] = true;
            vm.itemToEdit = angular.copy(censor);
        }

        function saveItem(censor) {
            vm.currentEdit[censor.id] = false;
            service.update(censor).then(function (data) {
                censor = data;
            });
        }
    }

    function createCensorController($modal, service, item) {
        var vm = this;

        vm.item = angular.copy(item);

        vm.close = close;
        vm.save = save;

        function close() {
            $modal.dismiss();
        }

        function save() {
            service.create(vm.item).then(function (data) {
                vm.item = data;
                $modal.close(vm.item);
            });
        }
    }
})()