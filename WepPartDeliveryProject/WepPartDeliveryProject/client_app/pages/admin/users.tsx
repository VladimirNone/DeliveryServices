import UserCard from "@/components/cards/UserCard";
import PanelToHandleUsers from "@/components/PanelToHandleUsers";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { FC, useEffect, useState } from "react";
import { Button } from "react-bootstrap";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories,
        }
    }
}

const Users: FC<{categories:categoryItem[]}> = ({ categories}) => {
    const [users, setUsers] = useState<profileInfo[]>([]);
    const [roles, setRoles] = useState<string[]>([]);

    const [markedUsers, setMarkedUsers] = useState<string[]>([]);

    //нулевая страница загружается при переходе на страницу
    const [page, setPage] = useState(0);
    const [pageEnded, setPageEnded] = useState(true);

    const [searchText, setSearchText] = useState<string>("");
    
    const handleChangeSearchedText = (searchedText:string) => {
        setSearchText(searchedText);
        setPage(0);
        setUsers([]);
    }

    const handleMarkUser = async (userId:string) => {
        setMarkedUsers(prevMarkedUsers => prevMarkedUsers.concat(userId));
    }

    const handleUnmarkUser = async (userId:string) => {
        setMarkedUsers(prevMarkedUsers => prevMarkedUsers.filter(h => h != userId));
    }

    const handleShowMoreUsers = async () => {
        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/getUsers?${searchText != "" ? "searchText=" + searchText + "&" : ""}page=${page}`, {
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
            }
        });
        const loadedData = await resp.json() as {users: profileInfo[], pageEnded: boolean};
    
        if(resp.ok){
            setPage(page + 1);
            setUsers(users.concat(loadedData.users));
            setPageEnded(loadedData.pageEnded);
        }
        else{
            setPageEnded(true);
        }
    }

    const handleBlockUser = async () => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/blockUsers`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify(markedUsers.map<{userId: string}>((value) => { return {userId: value} }))
        });

        if(resp1.ok){
            setUsers(prevUsers => prevUsers.map((value)=> {
                if(markedUsers.includes(value.id as string))
                    value.isBlocked = true; 
                return value;
            }))
        }
    }

    const handleUnblockUser = async () => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/unblockUsers`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify(markedUsers.map<{userId: string}>((value) => { return {userId: value} }))
        });

        if(resp1.ok){
            setUsers(prevUsers => prevUsers.map((value)=> {
                if(markedUsers.includes(value.id as string))
                    value.isBlocked = false; 
                return value;
            }))
        }
    }

    const handleAddRoleTokUser = async (newRole:string) => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/addUserRole`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify(markedUsers.map<{userId: string}>((value) => { return {userId: value, changeRole: newRole} }))
        });

        if(resp1.ok){
            setUsers(prevUsers => prevUsers.map((value)=> {
                if(markedUsers.includes(value.id as string) && !value.roles?.includes(newRole)) {
                    value.roles += ", " + newRole;
                }
                return value;
            }))
        }
    }

    const handleRemoveRoleTokUser = async (oldRole:string) => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/removeUserRole`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify(markedUsers.map<{userId: string}>((value) => { return {userId: value, changeRole: oldRole} }))
        });

        if(resp1.ok){
            setUsers(prevUsers => prevUsers.map((value)=> {
                if(markedUsers.includes(value.id as string)  && value.roles?.includes(oldRole)){
                    value.roles = (value.roles as string).replaceAll(", " + oldRole, "");
                }
                return value;
            }))
        }
    }

    useEffect(()=>{
        if(page == 0){
            handleShowMoreUsers();

            const fetchData = async () => {
                const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/getRoles`, {
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                    }
                });
                const loadedData = await resp.json() as string[];
            
                if(resp.ok){
                    setRoles(loadedData);
                }
            }
            fetchData();
        }
    },[page, searchText]);

    return (
        <ClientLayout categories={categories}>
            <PanelToHandleUsers roles={roles} changeSearchedText={handleChangeSearchedText} blockUsers={handleBlockUser} unblockUsers={handleUnblockUser} addRole={handleAddRoleTokUser} removeRole={handleRemoveRoleTokUser}/>
            {users.map((user, i)=> <UserCard key={i} userInfo={user} markUser={handleMarkUser} unmarkUser={handleUnmarkUser}/>)}
            {!pageEnded && (
                <div>
                    <Button className='btn btn-primary w-100 mt-2' onClick={handleShowMoreUsers}>
                        Показать больше
                    </Button>
                </div>)
            }
        </ClientLayout>
    );
}

export default Users;