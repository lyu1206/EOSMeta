local i = 0
local this = _this_object
local plate = Instance.new('EosShape')

print('Stage Created.............')

plate.Name = 'Floor'
print('#1');
plate.PType = Enum.PrimitiveType.Cube
print('#2');
plate:SetLocalScale(500,0.5,500)
print('#3');
plate.CanCollide = true
print('#4');
plate.Visible = false
print('#5');
plate.Parent = Services.Workspace
print('#6');

print('Added Workspace : ',Services.Workspace);

plate.OnCollisionEnter:Connect(function(sender,collider)
	local avatar = Services.Players.LocalPlayer.Humanoid
	print('Something droped....',tostring(collider))
	avatar:Move(0,13 * 10,0)
end)

--[[
    local plate = Instance.new('EosShape')
    plate.Name = 'CheckBox'
    plate.PType = Enum.PrimitiveType.Cube
    plate:SetLocalScale(2,2,2)
    plate:SetLocalPosition(2,1,0)
    plate.Parent = Services.Workspace
--]]

local plateonStage = 
{
    {sx = 6,sz = 6,x = 0,y = 6,z =0 },
    {sx = 4,sz = 4,x = 0,y = 6,z = 7 },
    {sx = 4,sz = 4,x = 0,y = 6,z = 7 + 7},
    {sx = 4,sz = 4,x = 0,y = 6,z = 7 + 7 + 7},
    {sx = 4,sz = 4,x = 0,y = 6,z = 7 + 7 + 7 + 7},
    {sx = 6,sz = 6,x = 0,y = 6,z = 7 + 7 + 7 + 7 + 7},

    {sx = 3.5,sz = 3.5 ,x = -7 ,y = 6,z = 7 + 7 + 7 + 7 + 7},
    {sx = 3.5,sz = 3.5 ,x = -7-7 ,y = 6,z = 7 + 7 + 7 + 7 + 7},
    {sx = 3.5,sz = 3.5 ,x = -7-7-7 ,y = 6,z = 7 + 7 + 7 + 7 + 7},
    {sx = 3.5,sz = 3.5 ,x = -7-7-7-7 ,y = 6,z = 7 + 7 + 7 + 7 + 7},
    {sx = 6,sz = 6,x = -7-7-7-7-7 ,y = 6,z = 7 + 7 + 7 + 7 + 7},

    {sx = 3.5,sz = 3.5,x = -7-7-7-7-7 ,y = 6,z = 7 + 7 + 7 + 7 },
    {sx = 3.5,sz = 3.5,x = -7-7-7-7-7 ,y = 6,z = 7 + 7 + 7  },
    {sx = 3.5,sz = 3.5,x = -7-7-7-7-7 ,y = 6,z = 7 + 7  },
    {sx = 3.5,sz = 3.5,x = -7-7-7-7-7 ,y = 6,z = 7 },
    {sx = 6,sz = 6,x = -7-7-7-7-7 ,y = 6,z = 0 },

    {sx = 3.8,sz = 3.8,x = -7-7-7-7 ,y = 6,z = 0 },
    {sx = 3.8,sz = 3.8,x = -7-7-7 ,y = 6,z = 0 },
    {sx = 3.8,sz = 3.8,x = -7-7 ,y = 6,z = 0 },
    {sx = 3.8,sz = 3.8,x = -7 ,y = 6,z = 0 },

}
for k,v in pairs(plateonStage) do
    local plate = Instance.new('EosShape')
    plate.Name = 'Board'
    plate.PType = Enum.PrimitiveType.Cube
    plate:SetLocalScale(v.sx*10,1*10,v.sz*10)
    plate:SetLocalPosition(v.x*10,v.y*10,v.z*10)
	plate.Layer = 9
    plate.Parent = Services.Workspace
end
--[[
while i<60 do
    i = i + 1
    print('second count:',i,' obj - ',tostring(this),' objname:',this.Name)
    coroutine.yield()
end
--]]
