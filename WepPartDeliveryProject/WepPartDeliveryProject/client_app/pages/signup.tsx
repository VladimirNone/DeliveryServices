import { ChangeEvent, Component, FormEvent } from "react"
import Link from 'next/link'
import { Col, Form, Row, Container, Button } from "react-bootstrap";
import { NextRouter, withRouter } from "next/router";
import { AuthContext } from "@/components/contexts/AuthContext";

type SignupState = {
    login: string,
    password: string,
    name: string,
    born: string,
    address: string,
    phoneNumber: string,
    secondPassword: string,
    success: boolean,
    errors: string[],
}

type WithRouterProps = {
    router: NextRouter,
}

function isValidDate(dateString:string) : boolean {
    const date:number = Date.parse(dateString);
    return !isNaN(date);
}

class Signup extends Component<WithRouterProps, SignupState> {
    declare context: React.ContextType<typeof AuthContext>
    static contextType = AuthContext; 

    state = {
        login: '',
        password: '',
        name: '',
        born: '',
        address: '',
        phoneNumber: '',
        secondPassword: '',
        success: false,
        errors: [],
    }

    handleInputData = (e: ChangeEvent<HTMLInputElement>): void => {
        const value:string = e.target.value;
        const name:string = e.target.name;
        
        this.setState(prevState => ({ ...prevState, [name]: value }));
    }

    handleSubmit = async (e:FormEvent) => {
        if(this.state.password == "" || this.state.login == "" || this.state.name == ""|| this.state.born == "" ){
            alert("Вы не заполнили все обязательные поля. Пожалуйста, заполните поля, отмеченные звездочкой")
            return;
        }
        if(this.state.password != this.state.secondPassword){
            alert("Вы ввели два разных пароля, пожалуйста, проверьте введенные пароли")
            return;
        }
        if(!isValidDate(this.state.born)){
            alert("Вы ввели не верную дату рождения, пожалуйста, проверьте введенные данные")
            return;
        }

        e.preventDefault();
        const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/auth/signup`, {
            method: "POST",
            credentials: "include",
            body: JSON.stringify({login:this.state.login, 
                                password:this.state.password, 
                                name: this.state.name,
                                phoneNumber: this.state.phoneNumber, 
                                address: this.state.address, 
                                born: this.state.born })
        });

        if(response.ok){
            this.context.toggleIsAuthed();
            this.props.router.push('/');
        }
        else{
            alert(await response.text());
        }
    }

    render() {
        return (
            <Container fluid={'xl'} className='mt-5 mb-5'>
                <h1>Регистрация</h1>
                <Form onSubmit={this.handleSubmit}>
                    <h3>Введите данные для аккаунта dot Food</h3>
                    <div className="text-danger">
                        {/* {this.state.errors.map((error, i) => <p key={i}>{error}</p>)} */}
                    </div>
                    <Row>
                        <Col sm={10} md={6} lg={4}>
                            <Form.Group className={`mb-2`}>
                                <Form.Label>Ваше имя<div className="text-danger d-inline">*</div></Form.Label>
                                <Form.Control type="text" placeholder="Введите свое имя" name="name" value={this.state.name} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Form.Group className={`mb-2`}>
                                <Form.Label>Дата рождения<div className="text-danger d-inline">*</div></Form.Label>
                                <Form.Control type="text" placeholder="Введите свою дату рождения" name="born" value={this.state.born} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Form.Group className={`mb-2`}>
                                <Form.Label>Ваш адрес</Form.Label>
                                <Form.Control type="text" placeholder="Введите свой адрес" name="address" value={this.state.address} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Form.Group className={`mb-2`}>
                                <Form.Label>Ваш номер телефона</Form.Label>
                                <Form.Control type="tel" placeholder="+7" maxLength={12} name="phoneNumber" value={this.state.phoneNumber} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Form.Group className={`mb-2`}>
                                <Form.Label>Email<div className="text-danger d-inline">*</div></Form.Label>
                                <Form.Control type="email" placeholder="Введите логин" name="login" value={this.state.login} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Form.Group className={`mb-3`}>
                                <Form.Label>Пароль<div className="text-danger d-inline">*</div></Form.Label>
                                <Form.Control type="password" placeholder="Введите пароль" name="password" value={this.state.password} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Form.Group className={`mb-3`}>
                                <Form.Label>Введите пароль еще раз<div className="text-danger d-inline">*</div></Form.Label>
                                <Form.Control type="password" placeholder="Введите пароль" name="secondePassword" value={this.state.password} onChange={this.handleInputData}/>
                            </Form.Group>
                            <Row>
                                <Col xs={6} className="g-0 pe-1 ps-2">
                                    <Button variant="primary" type="submit" className="w-100">
                                        Зарегестрироваться
                                    </Button>
                                </Col>
                                <Col xs={6} className="g-0 ps-1 pe-2">
                                    <Link href={'/login'} className="w-100">
                                        <Button variant="primary" className="w-100">
                                            Войти
                                        </Button>
                                    </Link>
                                </Col>
                            </Row>
                        </Col>
                    </Row>
                </Form>
            </Container>)
    }
};

export default withRouter(Signup);