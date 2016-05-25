//manager.js
//mark.lawrence

(function () {
    var controllerId = 'PeopleController';

    angular.module('app.people').controller(controllerId, mainController);

    mainController.$inject = ['logger', 'peopleService', '$uibModal', 'config'];

    function mainController(logger, service, $modal, config) {
        var vm = this;
        var keyCodes = config.keyCodes;

        vm.title = "People Manager";

        vm.currentEdit = null;
        vm.isBusy = false;
        vm.lastDeleted = null;
        vm.lastUpdated = null;

        vm.people = [];
        vm.searchModel = {
            page: 1,
            pageSize: 15,
            orderBy: 'Firstname',
            orderDirection: 'desc'
        };
        vm.searchTerm = '';

        var tableStateRef;

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
        }

        vm.addItem = function addItem() {
            $modal.open({
                templateUrl: '/app/people/views/person.html',
                //controller: ['logger', '$uibModalInstance', 'peopleService', 'item', 'vm', 'storage', editPersonController],
                controller: ['logger', '$uibModalInstance', 'peopleService', 'vm', 'storage', editPersonController],
                controllerAs: 'vm',
                resolve: {
                    vm: vm
                }
            }).result.then(function (data) {
                vm.people.unshift(data);
                logger.success('Successfully created ' + getFullName(data));
            });
        }

        vm.editItem = function editItem(item) {
            vm.currentEdit = item;
            $modal.open({
                templateUrl: '/app/people/views/person.html',
                //controller: ['logger', '$uibModalInstance', 'peopleService', 'item', 'vm', 'storage', editPersonController],
                controller: ['logger', '$uibModalInstance', 'peopleService', 'vm', 'storage', editPersonController],
                controllerAs: 'vm',
                resolve: {
                    vm: vm
                }
            }).result.then(function (data) {
                vm.lastUpdated = angular.copy(vm.currentEdit);
                angular.extend(item, data);
                logger.success('Successfully updated ' + getFullName(data));
            });
        }

        vm.deleteItem = function deleteItem(person) {
            service.remove(person.id).then(function (data) {
                vm.lastDeleted = person;
                var idx = vm.people.indexOf(person);
                vm.people.splice(idx, 1);
                logger.warning('Deleted person ' + person.firstname + ' ' + person.lastname);
            });
        }

        vm.undoDelete = function () {
            service.create(vm.lastDeleted).then(function (data) {
                logger.success('Successfully restored ' + data.firstname + ' ' + data.lastname);
                vm.people.unshift(data);
                vm.lastDeleted = null;
            });
        };

        vm.undoChange = function () {
            service.update(vm.lastUpdated)
                .then(function (data) {
                    angular.forEach(vm.people,
                        function (u, i) {
                            if (u.id === vm.lastUpdated.id) {
                                vm.people[i] = vm.lastUpdated;
                            }
                        });
                    logger.success('Successfully restored ' + vm.lastUpdated.firstname + ' ' + vm.lastUpdated.lastname);
                    vm.lastUpdated = null;
                });
        }

        vm.search = function search(tableState) {
            tableStateRef = tableState;
            if (!vm.searchModel.isPriority) vm.searchModel.isPriority = null;

            vm.searchModel.dateCreated = vm.daysFilter
                ? moment().subtract(parseInt(vm.daysFilter), 'days').format('MM/DD/YYYY')
                : null;

            if (typeof (tableState.sort.predicate) != "undefined") {
                vm.searchModel.orderBy = tableState.sort.predicate;
                vm.searchModel.orderDirection = tableState.sort.reverse ? 'desc' : 'asc';
            }
            if (typeof (tableState.search.predicateObject) != "undefined") {
                vm.daysFilter = tableState.search.predicateObject.dateCreated ? null : vm.daysFilter;
                vm.searchModel.dateCreated = vm.daysFilter
                    ? vm.searchModel.dateCreated
                    : tableState.search.predicateObject.dateCreated;

                if (tableState.search.predicateObject.fuzzyMatchValue) {
                    vm.searchModel.fuzzyMatchValue = tableState.search.predicateObject.fuzzyMatchValue / 100;
                } else {
                    vm.searchModel.fuzzyMatchValue = null;
                }

                vm.searchModel.isDonor = tableState.search.predicateObject.isDonor;
                vm.searchModel.firstname = tableState.search.predicateObject.firstname;
                vm.searchModel.lastname = tableState.search.predicateObject.lastname;
                vm.searchModel.zipcode = tableState.search.predicateObject.zipCode;
                vm.searchModel.emailAddress = tableState.search.predicateObject.emailAddress;
                vm.searchModel.isPriority = tableState.search.predicateObject.isPriority;
            }

            vm.isBusy = true;
            service.query(vm.searchModel).then(function (data) {
                vm.people = data.items;
                vm.searchModel = data;
                vm.isBusy = false;
            });
        }

        vm.pages = function paged(pageNum) {
            search(tableStateRef);
        }

        vm.quickFilter = function () {
            vm.search(tableStateRef);
        }

        function getFullName(person) {
            return person.firstname + ' ' + person.lastname;
        }
    }

    //function editPersonController(logger, $modal, service, item, model, storage) {
    function editPersonController(logger, $modal, service, model, storage) {
        var vm = this;
        //TODO: Can be made global??
        var censors = JSON.parse(storage.get('censors'));

        vm.item = angular.copy(model.currentEdit);

        vm.close = function () {
            $modal.dismiss();
        }

        vm.matchValue = function () {
            var value = 0.0;
            var fn = getFullName(vm.item);

            _.forEach(censors, function (censor) {
                var idx = clj_fuzzy.metrics.dice(fn, censor.word);
                if (idx > value) value = idx;
            });
            vm.item.fuzzyMatchValue = value;
        }

        vm.save = function () {
            if (vm.item.id) {
                service.update(vm.item).then(function (data) {
                    $modal.close(data);
                });
            } else {
                service.create(vm.item).then(function (data) {
                    $modal.close(data);
                });
            }
        }

        function getFullName(item) {
            return item.firstname + ' ' + item.lastname;
        }
    }
})()