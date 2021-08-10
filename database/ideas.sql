create table Users (user_id Int primary key, email varchar(255) not null, password varchar(255) not null, created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
create table Items(item_id int primary key, itemvalue varchar(255));
create table UserItemRelation(user int not null, item int not null, foreign key (item) references Items (item_id) on update restrict on delete cascade, foreign key (user) references Users (user_id) on update restrict on delete cascade , unique(item,user));




or :


create table if not exists Users (user_id Int primary key, email varchar(255) not null index, password varchar(255) not null, created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
create index email on Users(email);

create table if not exists Items(item_id int primary key, itemvalue varchar(255));

create table if not exists Roles(role_id int primary key, role_name varchar(255) not null);

create table if not exists UserRoleRelation(user_fk int not null, role_fk int not null,
foreign key (user_fk) references Users (user_id) on update restrict on delete cascade,
foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
unique(user_fk, role_fk) );


create table if not exists ItemRoleRelation(item_fk int not null, role_fk int not null,
foreign key (item_fk) references Items (item_id) on update restrict on delete cascade,
foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
unique(item_fk, role_fk) );