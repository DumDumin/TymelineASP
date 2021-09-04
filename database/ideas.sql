create table Users (user_id Int primary key, email varchar(255) not null, password varchar(255) not null, created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
create table Items(item_id int primary key, itemvalue varchar(255));
create table UserItemRelation(user int not null, item int not null, foreign key (item) references Items (item_id) on update restrict on delete cascade, foreign key (user) references Users (user_id) on update restrict on delete cascade , unique(item,user));




or :




create table Content ( id varchar(255) PRIMARY KEY , text varchar(255));
create table TymelineObjects ( id varchar(255) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId varchar(255) ,constraint fk_content foreign key (ContentId) references Content(id) on update restrict on Delete Cascade);

create table if not exists Users (user_id varchar(255) primary key, email varchar(255) not null index, password varchar(255) not null, created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
create index email on Users(email);


create table if not exists Roles(role_id int primary key, role_name varchar(255) not null, role_value varchar(255) not null);

create table if not exists UserRoleRelation(user_fk varchar(255) not null, role_fk int not null,
foreign key (user_fk) references Users (user_id) on update restrict on delete cascade,
foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
unique(user_fk, role_fk) );


create table if not exists ItemRoleRelation(item_fk varchar(64) not null, role_fk int not null,
foreign key (item_fk) references TymelineObjects (item_id) on update restrict on delete cascade,
foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
unique(item_fk, role_fk) );





-- selects:

select Users.user_id,Users.email,Users.password, Roles.role_name, Roles.role_value) From Users inner join UserRoleRelation on Users.role_id=UserItemRelation.user_fk inner join Roles on UserItemRelation.role_fk=Roles.role_id where Users.user_id="046d86a52ec5fef1850850374c9298ae";

-- updates:

Update Content c Inner Join TymelineObjects t ON (c.id=t.contentId)
                SET c.text="asdf",
                t.length=132,
                t.start=1231541,
                t.canChangeLength=false,
                t.canMove=true
                where t.id=
                "";


select Roles.role_id,Roles.role_name,Roles.role_value from Roles join UserRoleRelation ur on Roles.role_id=ur.role_fk join Users u on ur.user_fk=u.user_id where u.user_id="b6729fe89c8d88084b32d50b59240e4b" 