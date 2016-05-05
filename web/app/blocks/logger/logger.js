//mark.lawrence
//logger.js

(function () {
    'use strict';

    angular.module('blocks.logger').factory('logger', logger);

    logger.$inject = ['$log', 'toastr'];

    function logger($log, toastr) {
        var service = {
            showToasts: true,

            error: error,
            info: info,
            success: success,
            warning: warning,

            // straight to console; bypass toastr
            log: $log.log
        };

        return service;
        /////////////////////

        function error(message, data, title) {
            if (showToasts) toastr.error(message, title);
            $log.error('Error: ' + message, data);
        }

        function info(message, data, title, showToast) {
            toastr.info(message, title);
            $log.info('Info: ' + message, data);
        }

        function success(message, data, title) {
            if (showToasts) toastr.success(message, title);
            $log.info('Success: ' + message, data);
        }

        function warning(message, data, title) {
            if (showToasts) toastr.warning(message, title);
            $log.warn('Warning: ' + message, data);
        }
    }
})();