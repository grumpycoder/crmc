﻿//censor.service.js
//mark.lawrence

(function () {
    var serviceId = 'censorService';
    angular.module('app.service').factory(serviceId, ['$log', '$http', serviceController]);

    function serviceController(log, $http) {
        log.info('loaded ' + serviceId);
        var url = 'http://localhost:11277/api/censor/';

        var service = {
            get: get,
            query: query,
            update: update,
            remove: remove
        }

        return service;

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function query(searchTerm) {
            return $http.get(url + '?search=' + searchTerm)
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
    }
})()