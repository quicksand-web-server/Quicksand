class QuickSandFramework
{
	static #socket;
	static #controllerId = '';

	static main(controllerId)
	{
		QuickSandFramework.#controllerId = controllerId;
		QuickSandFramework.#socket = new WebSocket("ws://" + window.location.host + window.location.pathname);
		QuickSandFramework.#socket.onopen = QuickSandFramework.#onOpen;
		QuickSandFramework.#socket.onmessage = QuickSandFramework.#onMessage;
		QuickSandFramework.#socket.onclose = QuickSandFramework.#onClose;
		QuickSandFramework.#socket.onerror = QuickSandFramework.#onError;
	}

	static stop(code = 1000, reason = "")
	{
		QuickSandFramework.#socket.close(code, reason);
    }

	static send(content)
	{
		QuickSandFramework.#socket.send(content);
    }

	static #onOpen(event)
	{
		QuickSandFramework.#socket.send(QuickSandFramework.#controllerId);
		QuickSandFramework.#socket.send("Anchor: " + window.location.hash);
	}

	static #onMessage(event)
	{
		var data = JSON.parse(event.data);
		var requests = data["requests"];
		for (var i = 0; i < requests.length; ++i)
		{
			var request = requests[i];
			var requestName = request["name"];
			if (requestName == "attribute-added")
				document.getElementById(request["id"]).setAttribute(request["attribute"], request["value"]);
			else if (requestName == "attribute-removed")
				document.getElementById(request["id"]).removeAttribute(request["attribute"]);
			else if (requestName == "child-added")
			{
				var parent = document.getElementById(request["parentID"]);
				var child = document.createElement(request["childType"]);
				
				var childAttributes = request["childAttributes"];
				for (var n = 0; n < childAttributes.length; ++n)
				{
					var childAttribute = childAttributes[n];
					child.setAttribute(childAttribute["key"], childAttribute["value"]);
				}
				
				parent.insertBefore(child, parent.children[request["position"]]);
			}
			else if (requestName == "child-removed")
			{
				var parent = document.getElementById(request["parentID"]);
				parent.removeChild(parent.children[request["position"]]);
			}
			else if (requestName == "content-added")
			{
				var parent = document.getElementById(request["parentID"]);
				parent.insertBefore(document.createTextNode(request["content"]), parent.children[request["position"]]);
			}
		}
	}

	static #onClose(event)
	{
		if (event.wasClean)
			alert(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);
		else
			alert('[close] Connection died');
	}

	static #onError(error)
	{
		alert(`[error] ${error.message}`);
	}
};