//configuration.service.js
//mark.lawrence

(function () {
    'use strict';

    var serviceId = 'configurationService';
    angular.module('app.service').factory(serviceId, serviceController);

    function serviceController(logger, $http, config) {
        logger.log(serviceId + ' loaded ');
        var url = config.apiUrl + config.apiEndPoints.Configuration;

        var service = {
            get: get,
            update: update
        }

        return service;

        function get() {
            return $http.get(url).then(_success);
        }

        function update(config) {
            return $http.put(url, config).then(_success);
        }

        function _success(response) {
            return response.data;
        }
    }
})();