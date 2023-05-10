import PanelToHandleOrders from "@/components/PanelToHandleOrders";
import OrderCard from "@/components/cards/OrderCard";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { FC, useEffect, useState } from "react";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/kitchen/getOrderStates`);
    const states = await resp1.json() as orderState[];

    return {
        props: {
            categories,
            states,
        }
    }
}

const KitchenOrders: FC<{categories:categoryItem[], states:orderState[]}> = ({ categories, states }) => {
    const [orders, setOrders] = useState<orderCardInfo[]>([]);
    const [page, setPage] = useState(0);
    const [pageEnded, setPageEnded] = useState(true);
    const [selectedState, setSelectedState] = useState<orderState>(states[0]);

    const handleMoveOrderToPreviousStage = async (orderId:string, orderStateId:string) => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/moveToPreviousStage`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({ orderId })
        });

        if (resp1.ok) {
            setOrders(prevOrders => prevOrders.filter(el => el.id != orderId ));
        }
    }

    const handleMoveOrderToNextStage = async (orderId:string, orderStateId:string) => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/moveToNextStage`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({ orderId })
        });

        if (resp1.ok) {
            setOrders(prevOrders => prevOrders.filter(el => el.id != orderId ));
        }
    }

    const handleSelectState = (selectState: orderState) => {
        setSelectedState(selectState);
        setOrders([]);
        setPage(0);
    }

    const handleShowMoreOrders = async  () => {
        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/kitchen/getOrders?page=${page}&numberOfState=${selectedState.numberOfStage}`, {
            credentials: "include",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
            }
        });
        const loadedData = await resp.json() as {orders: orderCardInfo[], pageEnded: boolean};
    
        if(resp.ok){
            setPage(page + 1);
            setOrders(orders.concat(loadedData.orders));
            setPageEnded(loadedData.pageEnded);
        }
        else{
            setPageEnded(true);
        }
    }

    useEffect(()=>{
        if(page == 0){
            handleShowMoreOrders();
        }
    }, [page]);

    return (
        <ClientLayout categories={categories}>
            <PanelToHandleOrders orderStates={states} selectState={handleSelectState}/>
            {orders.map((order, i)=> <OrderCard key={i} {...order} DeleteOrder={()=>{}} MoveOrderToPreviousStage={handleMoveOrderToPreviousStage} MoveOrderToNextStage={handleMoveOrderToNextStage}/>)}
            {!pageEnded && (<div>
                    <button className='btn btn-primary w-100 mt-2' onClick={handleShowMoreOrders}>
                        Показать больше
                    </button>
                </div>)
            }
        </ClientLayout>
    );
}

export default KitchenOrders;