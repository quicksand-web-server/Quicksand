class QuickSandFramework
{
	static #socket;
	static #controlerId = '';

	static main(controlerId)
	{
		QuickSandFramework.#controlerId = controlerId;
		QuickSandFramework.#socket = new WebSocket("ws://" + window.location.host + window.location.pathname);
		QuickSandFramework.#socket.onopen = QuickSandFramework.#onOpen;
		QuickSandFramework.#socket.onmessage = QuickSandFramework.#onMessage;
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
		QuickSandFramework.#socket.send(QuickSandFramework.#controlerId);
		QuickSandFramework.#socket.send("Anchor: " + window.location.hash);
	}

	static #onMessage(event)
	{
		let data = JSON.parse(event.data);
		let requests = data["requests"];
		for (let i = 0; i < requests.length; ++i)
		{
			let request = requests[i];
			let requestName = request["name"];
			if (requestName == "attribute-added")
				document.getElementById(request["id"]).setAttribute(request["attribute"], request["value"]);
			else if (requestName == "attribute-removed")
				document.getElementById(request["id"]).removeAttribute(request["attribute"]);
			else if (requestName == "child-added")
			{
				let parent = document.getElementById(request["parentID"]);
				let child = document.createElement(request["childType"]);

				let childAttributes = request["childAttributes"];
				for (let n = 0; n < childAttributes.length; ++n)
				{
					let childAttribute = childAttributes[n];
					child.setAttribute(childAttribute["key"], childAttribute["value"]);
				}

				parent.insertBefore(child, parent.children[request["position"]]);
			}
			else if (requestName == "child-removed")
			{
				let parent = document.getElementById(request["parentID"]);
				parent.removeChild(parent.children[request["position"]]);
			}
			else if (requestName == "content-added")
			{
				let parent = document.getElementById(request["parentID"]);
				parent.insertBefore(document.createTextNode(request["content"]), parent.children[request["position"]]);
			}
			else if (requestName == "redirect")
			{
				let newHref = request["href"];
				if (request["no-back"])
					window.location.replace(newHref);
				else
					window.location.href = newHref;
            }
		}
	}
}