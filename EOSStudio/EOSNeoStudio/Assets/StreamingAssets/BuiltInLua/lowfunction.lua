function loadmodule(code,name,chunkname)
	local f,error = load(code,chunkname)
	if f then 
		_G[name] = f()
	else
		print(error)
	end
end

Enum = {}
Enum.PrimitiveType = {Sphere= 0,Cube = 3}


Instance = {}
function Instance.new(name)
	return InstanceNew(name)
end
