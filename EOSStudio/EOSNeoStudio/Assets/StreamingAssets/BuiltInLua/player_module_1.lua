local player = {}

local idx = 1
local updates = {}

function player.Update()
	for key, co in pairs(updates) do
		local s = coroutine.status(co.routine)
		if s == 'dead' then
			updates[key] = nil;
		elseif s == 'suspended' then
			coroutine.resume(co.routine)
		end
	end
end
function player.RegistRoutine(owner,code,chunk)
	local f,errmsg = load(code,chunk)
	if f == nil then		
		print(errmsg)
		return
	end
	local co = coroutine.create(f)
	_this_object = owner
	coroutine.resume(co)
	updates[idx] = {routine = co,this = owner}
	idx = idx + 1
end

Time = { deltaTime = 0 }

return player
