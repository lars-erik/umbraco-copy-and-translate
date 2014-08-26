angular.module("umbraco")
	.controller("MP.PasteAndTranslate.CopyController",
	function ($scope, eventsService, contentResource, navigationService, appState, treeService, umbRequestHelper, $http) {
        // custom start

	    function copyWithHookHeader(args, key) {
	        if (!args) {
	            throw "args cannot be null";
	        }
	        if (!args.parentId) {
	            throw "args.parentId cannot be null";
	        }
	        if (!args.id) {
	            throw "args.id cannot be null";
	        }

	        return umbRequestHelper.resourcePromise(
                $http.post("/umbraco/copyandtranslate/languages/PostCopy",
                    //umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostCopy"),
                    args, {
                        headers: {
                            translate: "true",
                            translateFrom: $scope.fromLanguage.Code,
                            translateTo: $scope.toLanguage.Code,
                            translateGuid: key
                        }
                    }),
                'Failed to copy content');
	    }

        function setReady() {
            $scope.pendingSelection = !$scope.fromLanguage || !$scope.toLanguage;
        }

	    $scope.pendingSelection = true;
	    $scope.showLanguages = false;
	    $scope.copying = false;

	    $scope.fromLanguage = null;
	    $scope.availableFromLanguages = [];
	    $scope.toLanguage = null;
	    $scope.availableToLanguages = [];

	    $scope.status = "";

	    $scope.$watch(function () { return $scope.fromLanguage; }, setReady);
	    $scope.$watch(function () { return $scope.toLanguage; }, setReady);

        // end: custom

	    var dialogOptions = $scope.$parent.dialogOptions;

	    $scope.relateToOriginal = false;
	    $scope.dialogTreeEventHandler = $({});

	    var node = dialogOptions.currentNode;

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", function (ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        eventsService.emit("editors.content.copyController.select", args);

	        var c = $(args.event.target.parentElement);
	        if ($scope.selectedEl) {
	            $scope.selectedEl.find(".temporary").remove();
	            $scope.selectedEl.find("i.umb-tree-icon").show();
	        }

	        var temp = "<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>";
	        var icon = c.find("i.umb-tree-icon");
	        if (icon.length > 0) {
	            icon.hide().after(temp);
	        } else {
	            c.prepend(temp);
	        }

	        $scope.target = args.node;
	        $scope.selectedEl = c;

            // custom start

	        $scope.showLanguages = false;
	        $http.get("/umbraco/copyandtranslate/languages/getlanguages", {
	            params: {
	                fromId: dialogOptions.currentNode.id,
	                toId: $scope.target.id
	            }
	        }).success(function (result) {
	            $scope.showLanguages = true;
	            $scope.availableFromLanguages = result[dialogOptions.currentNode.id];
	            $scope.fromLanguage = $scope.availableFromLanguages.length === 1 ? $scope.availableFromLanguages[0] : null;
	            $scope.availableToLanguages = result[$scope.target.id];
	            $scope.toLanguage = $scope.availableToLanguages.length === 1 ? $scope.availableToLanguages[0] : null;
	        });

	        // end: custom


	    });

	    function updateStatus(scope, key) {
	        $.ajax({
	            url: "/umbraco/copyandtranslate/languages/status",
	            method: "POST",

	            headers: {
	                translateGuid: key
	            },

	            success: function(status) {
	                scope.$apply(function() {
	                    scope.status = status;
	                });
	            }
	        });
        }

	    function startPolling(scope, key) {
	        updateStatus(scope, key);
            return setInterval(function() {
                updateStatus(scope, key);
            }, 1000);
        }

	    $scope.copy = function () {
	        // custom start
	        var interval,
	            promise;

	        $scope.copying = true;

	        promise = $http.post("/umbraco/copyandtranslate/languages/initialize")
	            .then(function(result) {
	                var key = JSON.parse(result.data);
	                interval = startPolling($scope, key);
	                return copyWithHookHeader({ parentId: $scope.target.id, id: node.id, relateToOriginal: $scope.relateToOriginal }, key);
	            })
	            .then(function(path) {
	                $scope.error = false;
	                $scope.success = true;

	                //get the currently edited node (if any)
	                var activeNode = appState.getTreeState("selectedNode");

	                //we need to do a double sync here: first sync to the copied content - but don't activate the node,
	                //then sync to the currenlty edited content (note: this might not be the content that was copied!!)

	                navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false }).then(function(args) {
	                    if (activeNode) {
	                        var activeNodePath = treeService.getPath(activeNode).join();
	                        //sync to this node now - depending on what was copied this might already be synced but might not be
	                        navigationService.syncTree({ tree: "content", path: activeNodePath, forceReload: false, activate: true });
	                    }
	                });

	            }, function(err) {
	                $scope.success = false;
	                $scope.error = err;
	            });

	        if (promise["always"])
	            promise["always"](function() {
	                clearInterval(interval);
	                $scope.copying = false;
	            });
	        else if (promise["finally"]);
	            promise["always"](function () {
	                clearInterval(interval);
	                $scope.copying = false;
	            });

	        // end: custom

	    };
	});