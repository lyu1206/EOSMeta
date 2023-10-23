local i = 0
local this = _this_object
local avatar = this.Parent;
print('Script Owner:',avatar.Name,' position:',avatar.LocalPosition)
print('---------------------:')
avatar:SetPosition(0,13*10,0)
avatar.CanCollide = true