//people.service.js
//mark.lawrence

(function () {
    var serviceId = 'peopleService';
    angular.module('app.service').factory(serviceId, ['$log', '$http', serviceController]);

    function serviceController(log, $http) {
        log.info(serviceId + ' loaded');
        var url = 'http://localhost:11277/api/person/';

        var service = {
            get: get,
            query: query,
            update: update,
            remove: remove,
            getCurrentStats: getCurrentStats
        }

        return service;

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function query(search) {
            return $http.post(url + 'search', search)
                .then(function (response) {
                    return response.data;
                });
        }

        function update(censor) {
            return $http.put(url, censor)
                .then(function (response) {
                    return response.data;
                });
        }

        function remove(id) {
            return $http.delete(url + id)
                .then(function (response) {
                    return response.data;
                });
        }

        function getCurrentStats() {
            return $http.get(url + 'stat')
                .then(function (response) {
                    return response.data;
                });
        }
    }
})()