import WorkerCard from "@/components/cards/WorkerCard";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { FC, useEffect, useState } from "react";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories,
        }
    }
}

const Workers: FC<{categories:categoryItem[]}> = ({ categories}) => {
    const [workers, setWorkers] = useState<profileInfo[]>([]);

    useEffect(()=>{
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/kitchen/getWorkers`, {
                credentials: "include",
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                }
            });
        
            if(resp.ok){
                const loadedData = await resp.json() as profileInfo[];
                setWorkers(loadedData);
            }
            else{
                alert(await resp.text());
            }
        }
        fetchData();
    },[]);

    return (
        <ClientLayout categories={categories}>
            {workers.map((user, i)=> <WorkerCard key={i} userInfo={user}/>)}
        </ClientLayout>
    );
}

export default Workers;