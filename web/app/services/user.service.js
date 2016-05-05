//user.service.js
//mark.lawrence

(function () {
    var serviceId = 'userService';
    angular.module('app.service').factory(serviceId, serviceController);

    serviceController.$inject = ['$log', '$http', 'config'];

    function serviceController(log, $http, config) {
        log.info(serviceId + ' loaded');
        var url = config.apiUrl + config.apiEndPoints.User;

        var service = {
            availableRoles: availableRoles,
            create: create,
            get: get,
            query: query,
            remove: remove,
            update: update
        }

        return service;

        function availableRoles() {
            return $http.get(url + '/roles')
                .then(function (response) {
                    return response.data;
                });
        }

        function create(user) {
            return $http.post(url, user)
                .then(function (response) {
                    return response.data;
                });
        }

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function query(searchTerm) {
            if (searchTerm != undefined) {
                return $http.get(url + '?searchTerm=' + searchTerm)
                    .then(function(response) {
                        return response.data;
                    });
            } else {
                return get(url);
            }
        }

        function update(user) {
            return $http.put(url, user)
                .then(function (response) {
                    return response.data;
                });
        }

        function remove(id) {
            return $http.delete(url + '/' + id)
                .then(function (response) {
                    return response.data;
                });
        }
    }
})()