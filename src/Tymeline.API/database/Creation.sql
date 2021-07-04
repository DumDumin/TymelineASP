create table Content ( id int auto_increment primary key , text varchar(255)); 
create table TymelineObjects ( id int auto_increment primary key, length int, start int, canChangeLength bool, canMove bool,ContentID int,constraint fk_content foreign key (ContentID) references Content(id) on update restrict on Delete Cascade);  
