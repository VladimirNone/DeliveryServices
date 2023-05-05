import OrderCard from "@/components/cards/OrderCard";
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

const Users: FC<{categories:categoryItem[]}> = ({ categories }) => {
    const [orders, setOrders] = useState<orderCardInfo[]>([]);
    //нулевая страница загружается при переходе на страницу
    const [page, setPage] = useState(0);
    const [pageEnded, setPageEnded] = useState(true);

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

            const changedOrderIndex = orders.findIndex(el => el.id == orderId);
            const updatedOrders = [...orders];
            const deletedStage = updatedOrders[changedOrderIndex].story?.pop();

            if(deletedStage?.orderStateId == orderStateId)
                setOrders(updatedOrders);
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
            const newOrderState = await resp1.json() as orderState;
            const changedOrderIndex = orders.findIndex(el => el.id == orderId);
            const updatedOrders = [...orders];
            updatedOrders[changedOrderIndex].story?.push(newOrderState);

            if (newOrderState.orderStateId != orderStateId)
                setOrders(updatedOrders);
        }
    }

    const handleShowMoreDishes = async  () => {
        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/getOrders?page=${page}`, {
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

    const handleDeleteItem = async (orderId:string) => {
        setOrders(prevOrders => prevOrders.filter(el => el.id != orderId ));

        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/cancelOrder`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({orderId})
        });
    }

    useEffect(()=>{
        if(page == 0){
            handleShowMoreDishes();
        }
    });

    return (
        <ClientLayout categories={categories}>
            {orders.map((order, i)=> <OrderCard key={i} {...order} DeleteOrder={handleDeleteItem} MoveOrderToPreviousStage={handleMoveOrderToPreviousStage} MoveOrderToNextStage={handleMoveOrderToNextStage}/>)}
            {!pageEnded && (<div>
                    <button className='btn btn-primary w-100 mt-2' onClick={handleShowMoreDishes}>
                        Показать больше
                    </button>
                </div>)
            }
        </ClientLayout>
    );
}

export default Users;